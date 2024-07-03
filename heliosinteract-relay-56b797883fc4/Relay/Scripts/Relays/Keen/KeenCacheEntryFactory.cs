namespace Helios.Relay.Keen
{
    public class KeenCacheEntryFactory : ICacheEntryFactory<KeenCacheEntry>
    {
        public ValidationResult CreateEntryFromPostData(IPostData postData, out KeenCacheEntry cacheEntry)
        {
            cacheEntry = new KeenCacheEntry(postData);
            return new ValidationResult(true, string.Empty);
        }
    }
}
