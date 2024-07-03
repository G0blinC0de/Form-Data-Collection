using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace Helios.Relay.GoogleUa
{
    public class GoogleUaWebRequest
    {
        private readonly RestClient _restClient;
        private readonly string _propertyId;

        private const int TIMEOUT_MILLISECONDS = 300000;
        private const int MAX_REDIRECTS = 3;

        public GoogleUaWebRequest(IConfiguration configuration)
        {
            _restClient = new RestClient("https://www.google-analytics.com");
            _restClient.Options.MaxTimeout = TIMEOUT_MILLISECONDS;
            _restClient.Options.MaxRedirects = MAX_REDIRECTS;
            _propertyId = configuration.GetSection("Google-UA")["PropertyId"];
        }

        public async Task SendEvent(GoogleUaEvent uaEvent, string guestId)
        {
            // Build url
            var builder = new System.Text.StringBuilder("collect");
            builder.Append($"?v=1&tid={_propertyId}&cid={guestId}");
            builder.Append(uaEvent.UrlParameters());
            var urlOptions = Uri.EscapeUriString(builder.ToString());

            // Send request
            var request = new RestRequest(urlOptions);
            var response = await _restClient.ExecuteAsync(request);

            // Handle error
            if (!response.IsSuccessful) throw new RequestFailureException("Failed to send event to Google.", response);
        }

        private void LogResponse(RestResponse response, string loggingKey)
        {
            LogResponse((int)response.StatusCode, response.StatusDescription, response.IsSuccessful, loggingKey);
        }

        private void LogResponse(int statusCode, string statusDescription, bool isSuccess, string loggingKey)
        {
            ConsoleLogger.WriteLine($"{(isSuccess ? "Request Success" : "Request Fail")}: {statusCode} {statusDescription}", RelayService.GoogleUA, loggingKey);
        }
    }
}
