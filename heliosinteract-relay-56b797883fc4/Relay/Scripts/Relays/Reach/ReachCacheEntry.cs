using Newtonsoft.Json;

namespace Helios.Relay.Reach
{
    public class ReachCacheEntry : CacheEntry
    {
        const string LOGGING_ID = "loggingId";
        const string CREATED = "created";
        
        [JsonConstructor]
        public ReachCacheEntry() { }

        public ReachCacheEntry(IPostData data) : base(true)
        {
            Experience = data.Experience;
            Guest = data.Guest;
            FileInfo = data.FileInfo;
            Key = data.Key;

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
    }
}