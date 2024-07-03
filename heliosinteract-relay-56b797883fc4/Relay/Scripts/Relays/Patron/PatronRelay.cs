using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Helios.Relay.Patron
{
    public sealed class PatronRelay : Relay<PatronCacheEntry>
    {
        public PatronRelay(ICache cache, IConfiguration config) : base(cache)
        {
            _webRequest = new PatronWebRequest(config);
            _entryFactory = new PatronCacheEntryFactory();
        }
        
        private readonly PatronWebRequest _webRequest;

        public override RelayService ServiceType => RelayService.Patron;

        protected override async Task HandleCacheEntry(PatronCacheEntry cacheEntry)
        {
            if (!_webRequest.IsAuthenticated)
            {
                Log("Authenticating...");
                await _webRequest.Authorize();
                Log("Successful authentication.");
            }

            Log($"Submitting entry...", cacheEntry);
            
            switch (cacheEntry.Type)
            {
                case PatronCacheEntry.PostType.Photo:
                    await _webRequest.PostPhoto(cacheEntry.GuestId, cacheEntry.FileInfo).ConfigureAwait(false);
                    break;

                case PatronCacheEntry.PostType.Video:
                    await _webRequest.PostVideo(cacheEntry.GuestId, cacheEntry.FileInfo).ConfigureAwait(false);
                    break;

                case PatronCacheEntry.PostType.Survey:
                    throw new RelayException("Received a Survey request, which is not yet implemented.");

                case PatronCacheEntry.PostType.CheckIn:
                    throw new RelayException("Received a Check-In request, which is not yet implemented.");

                default:
                    // Unreachable
                    throw new RelayException($"Unexpected error: Invalid PatronCacheEntry.EntryType: {(int)cacheEntry.Type}");
            }

            Log($"Entry submitted", cacheEntry);
        }
    }
}
