using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Helios.Relay.Reach;

namespace Helios.Relay.Twilio
{
    public sealed class TwilioRelay : Relay<TwilioCacheEntry>
    {
        public TwilioRelay(ICache cache, IConfiguration config) : base(cache)
        {
            _twilioWebRequest = new TwilioWebRequest(config);
            _reachWebRequest = new ReachWebRequest();
            _entryFactory = new TwilioCacheEntryFactory();
        }
        
        private readonly TwilioWebRequest _twilioWebRequest;
        private readonly ReachWebRequest _reachWebRequest;

        public override RelayService ServiceType => RelayService.Twilio;

        protected override async Task HandleCacheEntry(TwilioCacheEntry cacheEntry)
        {
            if (cacheEntry.Guest != null && string.IsNullOrEmpty(cacheEntry.Experience?.guestId))
            {
                await GuestSendAsync(cacheEntry).ConfigureAwait(false);
            }

            if (string.IsNullOrEmpty(cacheEntry.FileInfo.url))
            {
                await FileSendAsync(cacheEntry).ConfigureAwait(false);
            }

            while (cacheEntry.PendingPhoneList.Count > 0)
            {
                await TwilioSendAsync(cacheEntry).ConfigureAwait(false);
            }

            if (string.IsNullOrEmpty(cacheEntry.Experience?.id))
            {
                await ExperienceSendAsync(cacheEntry).ConfigureAwait(false);
            }
        }

        private async Task TwilioSendAsync(TwilioCacheEntry cacheEntry)
        {
            await _twilioWebRequest.SendTwilioMessageAsync(cacheEntry.FileInfo.url, cacheEntry.PendingPhoneList[0]).ConfigureAwait(false);
            cacheEntry.PendingPhoneList.RemoveAt(0);
            Log($"Successfully sent message to Twilio for #: {cacheEntry.PendingPhoneList[0]}", cacheEntry);
        }

        private async Task ExperienceSendAsync(TwilioCacheEntry cacheEntry)
        {
            Log("Attempting to send Experience.", cacheEntry);
            var (rExperience, message) = await _reachWebRequest.PostAsync(cacheEntry.Experience, cacheEntry.Key).ConfigureAwait(false);
        }

        private async Task FileSendAsync(TwilioCacheEntry cacheEntry)
        {
            if (!File.Exists(cacheEntry.FileInfo.path)) throw new RelayException($"No file found at path {cacheEntry.FileInfo.path}");

            Log("Attempting to send File.", cacheEntry);
            var (rFile, message) = await _reachWebRequest.PostFileAsync(cacheEntry.FileInfo, cacheEntry.Key).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(rFile?.id))
            {
                cacheEntry.FileInfo.url = rFile.url;
            }
            else
            {
                throw new RelayException(message);
            }
        }

        private async Task GuestSendAsync(TwilioCacheEntry cacheEntry)
        {
            Log("Attempting to send Guest.", cacheEntry);
            var (rGuest, message) = await _reachWebRequest.PostAsync(cacheEntry.Guest, cacheEntry.Key).ConfigureAwait(false);
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
            }
            else
            {
                throw new RelayException(message);
            }
        }
    }
}