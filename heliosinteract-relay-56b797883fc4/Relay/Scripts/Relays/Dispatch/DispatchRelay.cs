using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace Helios.Relay.Dispatch
{
    public sealed class DispatchRelay : Relay<DispatchCacheEntry>
    {
        private const int TIMEOUT_MILLISECONDS = 30000;

        private readonly RestClient _restClient;
        private readonly HttpClient _fileUploadClient;

        public DispatchRelay(ICache cache, IConfiguration config) : base(cache)
        {
            _restClient = new RestClient(config.GetSection("Dispatch")["Url"]);
            _restClient.Options.MaxTimeout = TIMEOUT_MILLISECONDS;
            _entryFactory = new DispatchCacheEntryFactory();

            _fileUploadClient = new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
        }

        public override RelayService ServiceType => RelayService.Dispatch;

        protected override async Task HandleCacheEntry(DispatchCacheEntry cacheEntry)
        {
            Log("Finding/creating guest and logging in...", cacheEntry);
            if (cacheEntry.Uid == null) await FindOrCreateGuest(cacheEntry).ConfigureAwait(false);
            if (cacheEntry.UserToken == null) await LoginGuest(cacheEntry).ConfigureAwait(false);

            Log("Creating post...", cacheEntry);
            await CreatePost(cacheEntry).ConfigureAwait(false);
            Log("Uploading file...", cacheEntry);
            await UploadFile(cacheEntry).ConfigureAwait(false);
            await SubmitPost(cacheEntry).ConfigureAwait(false);
            Log("Post submitted!", cacheEntry);
        }

        private async Task FindOrCreateGuest(DispatchCacheEntry cacheEntry)
        {
            // Prepare request
            var request = new RestRequest("users", Method.Post);
            request.AddHeader("Accept", "application/json");
            var jsonBody = JsonConvert.SerializeObject(new
            {
                name = cacheEntry.Name,
                email = cacheEntry.Email,
                phone = cacheEntry.Phone
            }, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            request.AddBody(jsonBody, "application/json");

            // Send request
            var response = await _restClient.ExecuteAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessful) throw new RequestFailureException("Failed to find/create guest.", response);

            // Deserialize response
            try
            {
                cacheEntry.Uid = JsonConvert.DeserializeAnonymousType(response.Content, new { uid = "" }).uid;
            }
            catch (JsonException ex)
            {
                throw new UnexpectedDataFormatException("Failed to deserialize find/create guest response.", ex);
            }
        }

        private async Task LoginGuest(DispatchCacheEntry cacheEntry)
        {
            // Prepare login request
            var request = new RestRequest("users/login", Method.Post);
            request.AddHeader("Accept", "application/json");
            request.AddJsonBody(new { uid = cacheEntry.Uid });

            // Send request
            var response = await _restClient.ExecuteAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessful) throw new RequestFailureException("Failed to find/create guest.", response);

            // Deserialize response
            try
            {
                cacheEntry.UserToken = JsonConvert.DeserializeAnonymousType(response.Content, new { token = "" }).token;
            }
            catch (JsonException ex)
            {
                throw new UnexpectedDataFormatException("Failed to deserialize guest login response.", ex);
            }
        }

        private async Task CreatePost(DispatchCacheEntry cacheEntry)
        {
            // Prepare the request
            var request = new RestRequest("posts", Method.Post);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", $"Bearer {cacheEntry.UserToken}");
            request.AddJsonBody(new
            {
                filename = cacheEntry.FileInfo.name,
                filetype = cacheEntry.FileInfo.mimeType,
                filesize = cacheEntry.FileInfo.length
            });

            // Send the request
            var response = await _restClient.ExecuteAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessful)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden) cacheEntry.UserToken = null;
                throw new RequestFailureException("Failed to create post.", response);
            }

            // Deserialize the response
            try
            {
                var postInfo = JsonConvert.DeserializeAnonymousType(response.Content, new
                {
                    _id = "",
                    signedPutUrl = "",
                    signedGetUrl = ""
                });
                cacheEntry.PostId = postInfo._id;
                cacheEntry.SignedPutUrl = postInfo.signedPutUrl;
            }
            catch (JsonException ex)
            {
                throw new UnexpectedDataFormatException("Failed to deserialize create post response.", ex);
            }
        }

        private async Task UploadFile(DispatchCacheEntry cacheEntry)
        {
            try
            {
                using var httpRequest = new HttpRequestMessage(HttpMethod.Put, cacheEntry.SignedPutUrl);
                using var fileStream = new FileStream(cacheEntry.FileInfo.path, FileMode.Open);
                httpRequest.Content = new StreamContent(fileStream);
                httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(cacheEntry.FileInfo.mimeType);
                httpRequest.Content.Headers.ContentLength = fileStream.Length;
                
                using var response = await _fileUploadClient.SendAsync(httpRequest).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode) throw new RequestFailureException("Failed to upload file.", response);
            }
            catch (RequestFailureException)
            {
                throw;
            }
            catch (IOException ex)
            {
                throw new RelayException("Failed to read cache file.", ex);
            }
            catch (Exception ex)
            {
                throw new RelayException("Failed to upload file.", ex);
            }
        }

        private async Task SubmitPost(DispatchCacheEntry cacheEntry)
        {
            // Prepare the request
            var request = new RestRequest($"posts/{cacheEntry.PostId}/submit", Method.Post);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", $"Bearer {cacheEntry.UserToken}");

            // Send the request
            var response = await _restClient.ExecuteAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessful)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden) cacheEntry.UserToken = null;
                throw new RequestFailureException("Failed to submit post.", response);
            }
        }
    }
}
