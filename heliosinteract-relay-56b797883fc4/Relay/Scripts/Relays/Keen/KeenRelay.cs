using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace Helios.Relay.Keen
{
    public sealed class KeenRelay : Relay<KeenCacheEntry>
    {
        private const int TIMEOUT_MILLISECONDS = 300000;
        private const int MAX_REDIRECTS = 3;

        public KeenRelay(ICache cache, IConfiguration config) : base(cache)
        {
            var projectId = config.GetSection("Keen")["ProjectId"];
            var eventCollection = config.GetSection("Keen")["EventCollection"];
            _restClient = new RestClient($"https://api.keen.io/3.0/projects/{projectId}/events/{eventCollection}");
            _restClient.Options.MaxTimeout = TIMEOUT_MILLISECONDS;
            _restClient.Options.MaxRedirects = MAX_REDIRECTS;
            _apiKey = config.GetSection("Keen")["ApiKey"];
            _entryFactory = new KeenCacheEntryFactory();
        }
        
        private readonly RestClient _restClient;
        private readonly string _apiKey;

        public override RelayService ServiceType => RelayService.Keen;

        protected override async Task HandleCacheEntry(KeenCacheEntry cacheEntry)
        {
            Log($"Uploading data...", cacheEntry);
            await SendPostToKeen(cacheEntry);
            Log($"Upload complete!", cacheEntry);
        }

        public async Task SendPostToKeen(KeenCacheEntry cacheEntry)
        {
            var request = new RestRequest();
            request.AddHeader("Authorization", _apiKey);
            request.AddBody(cacheEntry.Data, "application/json");

            var response = await _restClient.PostAsync(request);
            if (!response.IsSuccessful) throw new RequestFailureException("Failed to upload data.", response);
        }
    }
}
