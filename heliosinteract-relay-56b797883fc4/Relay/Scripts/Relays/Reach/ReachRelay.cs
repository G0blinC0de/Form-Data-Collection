using System.IO;
using System.Threading.Tasks;

namespace Helios.Relay.Reach
{
    public sealed class ReachRelay : Relay<ReachCacheEntry>
    {
        public ReachRelay(ICache cache) : base(cache)
        {
            _webRequest = new ReachWebRequest();
            _entryFactory = new ReachCacheEntryFactory();
        }
        
        private readonly ReachWebRequest _webRequest;

        public override RelayService ServiceType => RelayService.Reach;

        protected override async Task HandleCacheEntry(ReachCacheEntry cacheEntry)
        {
            if (cacheEntry.Guest != null && string.IsNullOrEmpty(cacheEntry.Experience?.guestId))
            {
                await GuestSendAsync(cacheEntry).ConfigureAwait(false);
            }

            if (cacheEntry.FileInfo != null && string.IsNullOrEmpty(cacheEntry.Experience?.fileId))
            {
                await FileSendAsync(cacheEntry).ConfigureAwait(false);
            }

            await ExperienceSendAsync(cacheEntry).ConfigureAwait(false);
        }

        private async Task ExperienceSendAsync(ReachCacheEntry cacheEntry)
        {
            Log("Attempting to send Experience.", cacheEntry);
            var (rExperience, message) = await _webRequest.PostAsync(cacheEntry.Experience, cacheEntry.Key).ConfigureAwait(false);
            if (string.IsNullOrEmpty(rExperience?.id)) throw new RelayException(message);
        }

        private async Task FileSendAsync(ReachCacheEntry cacheEntry)
        {
            if (!File.Exists(cacheEntry.FileInfo.path)) throw new RelayException($"No file found at path {cacheEntry.FileInfo.path}");

            Log("Sending File...", cacheEntry);
            var (rFile, message) = await _webRequest.PostFileAsync(cacheEntry.FileInfo, cacheEntry.Key).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(rFile?.id))
            {
                cacheEntry.Experience.fileId = rFile.id;
                cacheEntry.Experience.files = new[] {rFile};
                _cache.Update(cacheEntry);
            }
            else
            {
                throw new RelayException(message);
            }
        }

        private async Task GuestSendAsync(ReachCacheEntry cacheEntry)
        {
            Log("Sending Guest...", cacheEntry);
            var (rGuest, message) = await _webRequest.PostAsync(cacheEntry.Guest, cacheEntry.Key).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(rGuest?.id))
            {
                cacheEntry.Experience.guestId = rGuest.id;
                if (cacheEntry.FileInfo != null)
                {
                    if (cacheEntry.FileInfo.meta.ContainsKey("guestId"))
                    {
                        cacheEntry.FileInfo.meta["guestId"] = rGuest.id;
                    }
                    else
                    {
                        cacheEntry.FileInfo.meta.Add("guestId", rGuest.id);
                    }
                }

                _cache.Update(cacheEntry);
            }
            else
            {
                throw new RelayException(message);
            }
        }
    }
}