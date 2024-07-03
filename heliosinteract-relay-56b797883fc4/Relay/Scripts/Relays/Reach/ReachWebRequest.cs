using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace Helios.Relay.Reach
{
    public class ReachWebRequest
    {
        private readonly RestClient _restClient;
        private const int TIMEOUT_MILLISECONDS = 300000;
        private const int MAX_REDIRECTS = 3;

        public ReachWebRequest()
        {
            _restClient = new RestClient("https://reach.heliossf.com/api/");
            _restClient.Options.MaxTimeout = TIMEOUT_MILLISECONDS;
            _restClient.Options.MaxRedirects = MAX_REDIRECTS;
        }

        public Task<(T, string)> PostAsync<T>(T model, string key) where T : RModel
        {
            var request = ReachRequest($"/{model.endpoint}", key);
            var requestBody = JsonConvert.SerializeObject(model, new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
            request.AddJsonBody(requestBody);
            return ResponseTupleAsync(model, request);
        }

        public Task<(RFile, string)> PostFileAsync(RFile file, string key)
        {
            var request = ReachRequest(FormatFileRequestUrl(file, file.uploadOptions), key);
            request.AlwaysMultipartFormData = true;
            var bytes = File.ReadAllBytes(file.path);
            request.AddFile("file", bytes, file.name, file.mimeType);
            return ResponseTupleAsync(file, request);
        }

        private async Task<(T, string)> ResponseTupleAsync<T>(T model, RestRequest request) where T : RModel
        {
            var response = await _restClient.ExecuteAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessful) throw new RequestFailureException("", response);
            LogResponse(response, true, model.meta["loggingId"].ToString());
            var responseModel = response.Content.TrimStart().StartsWith("[") ? JsonConvert.DeserializeObject<T[]>(response.Content)[0] : JsonConvert.DeserializeObject<T>(response.Content);
            return (responseModel, "");
        }

        private void LogResponse(RestResponse response, bool isSuccess, string loggingKey)
        {
            ConsoleLogger.WriteLine($"{(isSuccess ? "Request Success" : "Request Failure")}: {response.StatusCode} {response.StatusDescription}", RelayService.Dispatch, loggingKey);
        }

        private RestRequest ReachRequest(string url, string key)
        {
            var request = new RestRequest(url, Method.Post);
            request.AddHeader("X-Helios-ID", key);
            request.AddHeader("Accept", "application/json");
            return request;
        }

        private string FormatFileRequestUrl(RFile file, RFileUploadOptions fileUploadOptions)
        {
            var url = $"/{file.endpoint}/upload/{file.activationId}";

            var meta = JsonConvert.SerializeObject(file.meta, new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
            string extra = null;
            if (fileUploadOptions != null)
            {
                extra = JsonConvert.SerializeObject(fileUploadOptions, new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
            }
            if (!string.IsNullOrEmpty(extra))
            {
                meta = string.Concat(meta, extra).Replace("}{", ",");
            }
            url = string.Concat(url, "?options=", meta);
            return url;
        }
    }
}