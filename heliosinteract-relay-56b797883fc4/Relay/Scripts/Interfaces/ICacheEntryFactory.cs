namespace Helios.Relay
{
    public interface ICacheEntryFactory<T> where T : CacheEntry
    {
        ValidationResult CreateEntryFromPostData(IPostData postData, out T cacheEntry);
    }
}
