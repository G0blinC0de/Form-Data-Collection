using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Helios.Encryption;

namespace Helios.Relay.Eshots
{
    public sealed class EshotsRelay : Relay<EshotsCacheEntry>
    {
        public override RelayService ServiceType => RelayService.Eshots;
        
        public EshotsRelay(ICache cache, IConfiguration config) : base(cache)
        {
            _restClient = new RestClient(ESHOTS_URL);
            _username = config.GetSection("Eshots")["Username"];
            _password = StringEncryption.Decrypt(config.GetSection("Eshots")["Password"]);

            _clientLicenseId = config.GetSection("Eshots")["ClientLicenseId"];

            _requestLogger = new RequestLogger(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "EshotsRequests.yml"));
            _entryFactory = new EshotsCacheEntryFactory(config);
        }

        private const string ESHOTS_URL = "https://eshots.io/";

        private string _clientLicenseId;
        private string _username;
        private string _password;
        private string _refreshToken;
        private string _accessToken;

        private bool _hasRefreshToken;
        private bool _isAuthenticated;
        private Dictionary<string, int> _eventDayIds = new Dictionary<string, int>();

        private readonly RestClient _restClient;
        private readonly RequestLogger _requestLogger;

        protected override async Task HandleCacheEntry(EshotsCacheEntry cacheEntry)
        {
            if (!_isAuthenticated)
            {
                if (!_hasRefreshToken)
                {
                    Log("Authenticating...");
                    await Authenticate();
                    Log("Successful authentication.");

                    Log("Getting eventDayIds...");
                    await GetEventDayIds();
                    Log("Retrieved eventDayIds.");
                }
                else
                {
                    Log("Refreshing token...");
                    await AuthenticateRefresh();
                    Log("Successful token refresh.");
                }
            }

            Log($"Submitting entry.", cacheEntry);

            // Confirm that the event day exists for this entry
            if (!_eventDayIds.ContainsKey(cacheEntry.CreateDtmShort)) throw new RelayException($"No eventDayId found for date string: {cacheEntry.CreateDtmShort}");

            // Post data
            if (cacheEntry.FileInfo != null && !cacheEntry.FilePosted)
            {
                // If a file is attached, send as a media post
                await PostMedia(cacheEntry);
            }
            else
            {
                // If no file is attached, send as a check-in/survey post
                await PostConsumerData(cacheEntry);
            }

            Log($"Entry submitted.", cacheEntry);
        }

        private async Task Authenticate()
        {
            // Send authentication request
            var request = new RestRequest("/api/v1/auth", Method.Post);
            request.AddJsonBody(new { username = _username, password = _password });
            var response = await _requestLogger.SendAndLogRequest(_restClient, request);
            if (!response.IsSuccessful) throw new RequestFailureException("The authorization request failed.", response);
            
            // Validate and extract data
            var responseContent = JObject.Parse(response.Content);
            var refreshToken = responseContent.Property("refresh_token");
            if (refreshToken == null) throw UnexpectedDataFormatException.MissingProperty("refresh_token");
            var accessToken = responseContent.Property("access_token");
            if (accessToken == null) throw UnexpectedDataFormatException.MissingProperty("access_token");

            // Set tokens and auth state
            _refreshToken = refreshToken.Value.Value<string>();
            _accessToken = accessToken.Value.Value<string>();
            _hasRefreshToken = true;
            _isAuthenticated = true;
        }

        private async Task AuthenticateRefresh()
        {
            // Send authentication refresh request
            var request = new RestRequest("/api/v1/auth/refresh", Method.Post);
            request.AddHeader("Authorization", $"Bearer {_refreshToken}");
            var response = await _requestLogger.SendAndLogRequest(_restClient, request);
            if (!response.IsSuccessful)
            {
                // 401 or 403 indicate a bad refresh token
                if (response.StatusCode == HttpStatusCode.Unauthorized ||
                    response.StatusCode == HttpStatusCode.Forbidden) _hasRefreshToken = false;
                throw new RequestFailureException("The token refresh request failed.", response);
            }

            // Validate and extract data
            var accessToken = JObject.Parse(response.Content).Property("access_token");
            if (accessToken == null) throw UnexpectedDataFormatException.MissingProperty("access_token");

            // Set token and auth state
            _accessToken = accessToken.Value<string>();
            _isAuthenticated = true;
        }

        private async Task GetEventDayIds()
        {
            // Send eventdays request
            var request = new RestRequest(
                $"api/v1/events/eventdays?clientLicenseID={_clientLicenseId}",
                Method.Get);
            request.AddHeader("Authorization", $"Bearer {_accessToken}");
            var response = await _requestLogger.SendAndLogRequest(_restClient, request);
            if (!response.IsSuccessful)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized) _isAuthenticated = false;
                if (response.StatusCode == HttpStatusCode.Forbidden) _hasRefreshToken = false;
                throw new RequestFailureException("The event days request failed.", response);
            }

            // Extract event day data from response
            _eventDayIds = new Dictionary<string, int>();
            try
            {
                var data = JObject.Parse(response.Content);
                data["events"].Select(eventData => (JObject)eventData["eventDays"])
                    .SelectMany(eventDayObj => eventDayObj.Children<JProperty>())
                    .ForEach(eventDayProp => _eventDayIds[eventDayProp.Name] = eventDayProp.Value.Value<int>());
            }
            catch (JsonException ex)
            {
                throw new UnexpectedDataFormatException("An error occurred while evaluating eventDayIds.", ex);
            }
        }

        private async Task PostMedia(EshotsCacheEntry cacheEntry)
        {
            // Get the file as a base64 encoded string
            var bytes = await File.ReadAllBytesAsync(cacheEntry.FileInfo.path);
            var base64String = Convert.ToBase64String(bytes);

            // Send the media post request
            var request = new RestRequest("/api/v2/media", Method.Post);
            request.AddHeader("Authorization", $"Bearer {_refreshToken}");
            request.AddJsonBody(new {
                media = new[] {
                    new {
                        eventDayId = _eventDayIds[cacheEntry.CreateDtmShort],
                        rElatId = cacheEntry.RElatId,
                        interactions = new[] {
                            new {
                                createDtm = cacheEntry.CreateDtmLong,
                                binary = base64String,
                                fileName = cacheEntry.FileInfo.name,
                                consumers = new[] {
                                    new { uId = cacheEntry.EventTokenId }
                                }
                            }
                        }
                    }
                }
            });
            var response = await _requestLogger.SendAndLogRequest(_restClient, request);
            if (!response.IsSuccessful)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized) _isAuthenticated = false;
                if (response.StatusCode == HttpStatusCode.Forbidden) _hasRefreshToken = false;
                throw new RequestFailureException("The media post request failed.", response);
            }
            cacheEntry.FilePosted = true;
        }

        private async Task PostConsumerData(EshotsCacheEntry cacheEntry)
        {
            // Send the consumer post request
            var request = new RestRequest("/api/v2/consumers", Method.Post);
            request.AddHeader("Authorization", $"Bearer {_refreshToken}");
            request.AddJsonBody(new
            {
                consumers = new[] {
                    new {
                        eventTokenId = cacheEntry.EventTokenId,
                        eventDayId = _eventDayIds[cacheEntry.CreateDtmShort],
                        rElatId = cacheEntry.RElatId,
                        interactions = new[] {
                            new {
                                createDtm = cacheEntry.CreateDtmLong,
                                answers = cacheEntry.Data ?? new Dictionary<string, string>()
                            }
                        }
                    }
                }
            });
            var response = await _requestLogger.SendAndLogRequest(_restClient, request);
            if (!response.IsSuccessful)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized) _isAuthenticated = false;
                if (response.StatusCode == HttpStatusCode.Forbidden) _hasRefreshToken = false;
                throw new RequestFailureException("The consumer post request failed.", response);
            }
            cacheEntry.DataPosted = true;
        }
    }
}
