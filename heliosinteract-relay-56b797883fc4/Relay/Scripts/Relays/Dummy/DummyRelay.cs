using LiteDB;
using System.Collections.Generic;

namespace Helios.Relay.Dummy
{
    public sealed class DummyRelay : IRelay
    {
        public RelayService ServiceType => RelayService.Dummy;

        public DummyRelay()
        {
            ConsoleLogger.WriteLine($"Relay online: {ServiceType}");
        }

        public void StartRelay() { }

        public ValidationResult TryValidate(IPostData postData, out CacheEntry cacheEntry)
        {
            cacheEntry = null;
            return new ValidationResult(true, string.Empty);
        }

        public void Add(CacheEntry cacheEntry) { }

        public bool Remove(BsonValue entryId) => true;

        public int RemoveFailedEntries(int minimumNumberOfFailures) => 0;

        public IEnumerable<CacheEntry> GetAllEntries()
        {
            yield break;
        }

        public IEnumerable<string> GetLogs()
        {
            yield break;
        }
    }
}
