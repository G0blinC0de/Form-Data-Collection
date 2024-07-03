using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Helios.Relay.Polygon
{
    public sealed class PolygonRelay : Relay<PolygonCacheEntry>
    {
        public PolygonRelay(ICache cache, IConfiguration config) : base(cache)
        {
            _webRequest = new PolygonWebRequest(config);
            _maxGasPrice = double.Parse(config.GetSection("Polygon")["MaxGasPrice"]);
            _entryFactory = new PolygonCacheEntryFactory(new[] { "walletAddress", "background", "eventName" });
        }

        private readonly PolygonWebRequest _webRequest;
        private readonly double _maxGasPrice;

        public override RelayService ServiceType => RelayService.Polygon;

        protected override async Task HandleCacheEntry(PolygonCacheEntry cacheEntry)
        {
            if (!_webRequest.IsAuthenticated)
            {
                Log("Authenticating...");
                await _webRequest.Authorize();
                Log("Authenticated");
            }

            // Check the current gas
            Log("Checking current Gas...", cacheEntry);
            var gasPrice = await _webRequest.GetGasEstimate();

            // Compare the gas price to the configured maximum
            if (gasPrice > _maxGasPrice) throw new RelayException("Gas price is too high.");

            // Send the file to the Mint endpoint
            Log("Sending to Mint endpoint...", cacheEntry);
            await _webRequest.Mint(cacheEntry);
            Log("Minted!");
        }
    }
}
