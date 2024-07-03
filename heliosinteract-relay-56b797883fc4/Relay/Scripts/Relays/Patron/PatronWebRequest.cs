using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace Helios.Relay.Patron
{
    public class PatronWebRequest
    {
        public PatronWebRequest(IConfiguration configuration)
        {
            _restClient = new RestClient(configuration.GetSection("Patron")["BaseUrl"]);
            _email = configuration.GetSection("Patron")["DeveloperEmail"];
            _password = Encryption.StringEncryption.Decrypt(configuration.GetSection("Patron")["DeveloperPassword"]);
            _brandId = configuration.GetSection("Patron")["BrandId"];
            _eventId = configuration.GetSection("Patron")["EventId"];
            _photoStreamId = configuration.GetSection("Patron")["PhotoStreamId"];
            _videoStreamId = configuration.GetSection("Patron")["VideoStreamId"];

            _requestLogger = new RequestLogger(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "patronRequests.yml"));
        }

        private readonly RestClient _restClient;
        private string _email;
        private string _password;
        private string _token;
        private string _brandId;
        private string _eventId;
        private string _photoStreamId;
        private string _videoStreamId;

        public bool IsAuthenticated { get; private set; }

        private readonly RequestLogger _requestLogger;

        public async Task Authorize()
        {
            var request = new RestRequest("/api/authorization/user", Method.Post);
            request.AddJsonBody(new { email = _email, password = _password });
            var response = await _restClient.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                _token = response.Content.Trim('"');
                IsAuthenticated = true;
            }
            else
            {
                throw new RequestFailureException("Failed to authenticate.");
            }
        }

        public async Task PostPhoto(string guestToken, RFile fileInfo)
        {
            var restRequest = new RestRequest($"/api/brand/{_brandId}/event/{_eventId}/stream/{_photoStreamId}/attendee/{guestToken}/photos", Method.Post);
            restRequest.AddHeader("Authorization", $"Bearer {_token}");
            restRequest.AddFile("File", fileInfo.path, fileInfo.mimeType);
            var restResponse = await _requestLogger.SendAndLogRequest(_restClient, restRequest, fileInfo);
            if (!restResponse.IsSuccessful)
            {
                if (restResponse.StatusCode == HttpStatusCode.Unauthorized) IsAuthenticated = false;
                throw new RequestFailureException("Failed to post photo", restResponse);
            }
        }

        public async Task PostVideo(string guestToken, RFile fileInfo)
        {
            var restRequest = new RestRequest($"/api/brand/{_brandId}/event/{_eventId}/stream/{_videoStreamId}/attendee/{guestToken}/videos", Method.Post);
            restRequest.AddHeader("Authorization", $"Bearer {_token}");
            restRequest.AddFile("File", fileInfo.path, fileInfo.mimeType);
            var restResponse = await _requestLogger.SendAndLogRequest(_restClient, restRequest, fileInfo);
            if (!restResponse.IsSuccessful)
            {
                if (restResponse.StatusCode == HttpStatusCode.Unauthorized) IsAuthenticated = false;
                throw new RequestFailureException("Failed to post video", restResponse);
            }
        }
    }
}
