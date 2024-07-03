using Newtonsoft.Json;

namespace Helios.Relay.Patron
{
    public class PatronCacheEntry : CacheEntry
    {
        [JsonConstructor]
        public PatronCacheEntry() { }

        public PatronCacheEntry(string guestId, PostType type, RFile fileInfo) : base(true)
        {
            GuestId = guestId;
            Type = type;
            FileInfo = fileInfo;
        }

        public PostType Type { get; private set; }
        public string GuestId { get; private set; }
        public RFile FileInfo { get; private set; }

        public enum PostType
        {
            Photo,
            Video,
            Survey,
            CheckIn
        }
    }
}
