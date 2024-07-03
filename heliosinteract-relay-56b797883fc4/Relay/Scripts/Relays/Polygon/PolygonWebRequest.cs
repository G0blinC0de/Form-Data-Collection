using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace Helios.Relay.Polygon
{
    public class PolygonWebRequest
    {
        private RestClient _restClient;
        private string _user;
        private string _password;
        private string _token;

        public bool IsAuthenticated { get; private set; }

        public PolygonWebRequest(IConfiguration configuration)
        {
            _restClient = new RestClient(configuration.GetSection("Polygon")["BaseUrl"]);
            _user = configuration.GetSection("Polygon")["User"];
            _password = Encryption.StringEncryption.Decrypt(configuration.GetSection("Polygon")["Password"]);
        }

        public async Task Authorize()
        {
            var request = new RestRequest("Token", Method.Post);
            request.AddJsonBody(new { User = _user, Password = _password });
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

        public async Task<double> GetGasEstimate()
        {
            var request = new RestRequest("Mint/GasEstimate", Method.Get);
            request.AddHeader("Authorization", $"Bearer {_token}");
            var response = await _restClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden) IsAuthenticated = false;
                throw new RequestFailureException("Failed to get gas estimate.", response);
            }
            return double.Parse(JsonConvert.DeserializeAnonymousType(response.Content, new { result = new { fastGasPrice = "" } }).result.fastGasPrice);
        }

        public async Task Mint(PolygonCacheEntry cacheEntry)
        {
            var request = new RestRequest("Mint", Method.Post);
            request.AddHeader("Authorization", $"Bearer {_token}");
            foreach (var field in cacheEntry.Fields) request.AddParameter(field.Key, field.Value);
            request.AddParameter("fileType", cacheEntry.FileInfo.mimeType);
            request.AddFile("file", cacheEntry.FileInfo.path);
            var response = await _restClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden) IsAuthenticated = false;
                throw new RequestFailureException("Failed to mint token.", response);
            }
        }
    }
}
