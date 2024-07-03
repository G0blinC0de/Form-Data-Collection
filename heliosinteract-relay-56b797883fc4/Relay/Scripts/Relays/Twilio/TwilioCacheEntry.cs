using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Helios.Relay.Twilio
{
    public class TwilioCacheEntry : CacheEntry
    {
        const string LOGGING_ID = "loggingId";
        const string CREATED = "created";

        [JsonConstructor]
        public TwilioCacheEntry() { }
        public TwilioCacheEntry(IPostData data, List<string> pendingPhoneList) : base(true)
        {
            Experience = data.Experience;
            Guest = data.Guest;
            FileInfo = data.FileInfo;
            Key = data.Key;
            PendingPhoneList = pendingPhoneList;

            Experience.AddMetaField(LOGGING_ID, EntryId);
            Experience.AddMetaField(CREATED, TimeCreated);
            Guest?.AddMetaField(LOGGING_ID, EntryId);
            Guest?.AddMetaField(CREATED, TimeCreated);
            FileInfo?.AddMetaField(LOGGING_ID, EntryId);
            FileInfo?.AddMetaField(CREATED, TimeCreated);
        }

        public RExperience Experience { get; private set; }
        public RGuest Guest { get; private set; }
        public RFile FileInfo { get; private set; }
        public string Key { get; private set; }

        public bool ShouldFreeze { get; set; }

        public List<string> PendingPhoneList { get; set; } = new List<string>();
    }
}