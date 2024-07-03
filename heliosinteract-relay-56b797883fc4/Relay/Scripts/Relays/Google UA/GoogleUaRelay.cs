using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Helios.Relay.GoogleUa
{
    public sealed class GoogleUaRelay : Relay<GoogleUaCacheEntry>
    {
        public GoogleUaRelay(ICache cache, IConfiguration config) : base(cache)
        {
            _webRequest = new GoogleUaWebRequest(config);
            _validator = new GoogleUaCacheEntryValidator(config);
        }

        private readonly GoogleUaWebRequest _webRequest;
        private readonly GoogleUaCacheEntryValidator _validator;

        public override RelayService ServiceType => RelayService.GoogleUA;

        protected override async Task HandleCacheEntry(GoogleUaCacheEntry cacheEntry)
        {
            // Get or generate the guest id
            var guestId = GetUserId(cacheEntry);

            // Try to send each event in the cache entry
            for (var i = cacheEntry.Events.Count - 1; i >= 0; i--)
            {
                await _webRequest.SendEvent(cacheEntry.Events[i], guestId).ConfigureAwait(false);
                cacheEntry.Events.RemoveAt(i);
            }

            static string GetUserId(GoogleUaCacheEntry cacheEntry)
            {
                if (!string.IsNullOrEmpty(cacheEntry.Experience?.guestId)) return cacheEntry.Experience?.guestId;
                if (!string.IsNullOrEmpty(cacheEntry.Guest?.id)) return cacheEntry.Guest?.id;
                return Guid.NewGuid().ToString();
            }
        }

        public override ValidationResult TryValidate(IPostData postData, out CacheEntry cacheEntry)
        {
            cacheEntry = null;

            // Validate event information in cacheEntry
            var validation = _validator.CreateEntryFromPostData(postData, out var googleUaEntry);

            if (validation.IsSuccess)
            {
                googleUaEntry = new GoogleUaCacheEntry(postData);
                _validator.BuidlUaEvents(googleUaEntry).ForEach(uaEvent => googleUaEntry.Events.Add(uaEvent));
            }

            cacheEntry = googleUaEntry;
            return validation;
        }
    }
}
