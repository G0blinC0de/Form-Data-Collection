using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using LiteDB;
using System;

namespace Helios.Relay
{
    public abstract class Relay<T> : IRelay where T : CacheEntry
    {
        private const int WAIT_BEFORE_RETRY_MILLISECONDS = 5000;
        
        protected Relay(ICache cache)
        {
            _cache = cache;
            ConsoleLogger.WriteLine($"Relay online: {ServiceType}");
        }
        
        private Task _sendTask;
        
        protected ICache _cache;
        protected ICacheEntryFactory<T> _entryFactory;

        public abstract RelayService ServiceType { get; }

        public void StartRelay()
        {
            _sendTask = _sendTask == null || _sendTask.IsCompleted ? Task.Run(RelayRoutineAsync) : _sendTask;
        }

        public void Add(CacheEntry cacheEntry)
        {
            _cache.Insert((T)cacheEntry);
            StartRelay();
        }

        public bool Remove(BsonValue entryId) => _cache.Remove<T>(entryId);

        public int RemoveFailedEntries(int minimumNumberOfFailures) => _cache.RemoveFailedEntries<T>(minimumNumberOfFailures);

        public IEnumerable<CacheEntry> GetAllEntries() => _cache.GetAllEntries<T>();

        public IEnumerable<string> GetLogs()
        {
            var entries = _cache.GetAllEntries<T>();
            return entries.Select((cacheEntry, i) => $"Id: {cacheEntry.EntryId} Attempts: {cacheEntry.TotalFailedAttempts} Last message: {cacheEntry.FailedMessage}");
        }

        public virtual ValidationResult TryValidate(IPostData postData, out CacheEntry cacheEntry)
        {
            var validation = _entryFactory.CreateEntryFromPostData(postData, out var typedCacheEntry);
            cacheEntry = validation.IsSuccess ? typedCacheEntry : null;
            return validation;
        }

        protected abstract Task HandleCacheEntry(T cacheEntry);

        protected void Log(string message, CacheEntry cacheEntry = null) => ConsoleLogger.WriteLine(message, ServiceType, cacheEntry?.EntryId);

        protected void LogError(string message, CacheEntry cacheEntry = null) => ConsoleLogger.WriteLine(message, ServiceType, cacheEntry?.EntryId, true, ConsoleColor.Red);

        private async Task RelayRoutineAsync()
        {
            while (_cache.Count<T>() > 0)
            {
                T cacheEntry = null;
                try
                {
                    cacheEntry = _cache.GetEntry<T>();
                    await HandleCacheEntry(cacheEntry);
                    _cache.Remove<T>(cacheEntry.EntryId);
                }
                catch (RelayException eshotsEx)
                {
                    LogError($"{ServiceType} error: {eshotsEx}", cacheEntry);
                    if (cacheEntry != null) FailedToSend(cacheEntry);
                    await Task.Delay(WAIT_BEFORE_RETRY_MILLISECONDS).ConfigureAwait(false);
                }
                catch (RequestFailureException requestEx)
                {
                    LogError($"Failed request: {requestEx}", cacheEntry);
                    if (cacheEntry != null) FailedToSend(cacheEntry);
                    await Task.Delay(WAIT_BEFORE_RETRY_MILLISECONDS).ConfigureAwait(false);
                }
                catch (UnexpectedDataFormatException dataFormatEx)
                {
                    LogError($"Received unexpected data from remote service.");
                    ConsoleLogger.WriteLine(dataFormatEx, true, ConsoleColor.Red);
                    if (cacheEntry != null) FailedToSend(cacheEntry);
                    await Task.Delay(WAIT_BEFORE_RETRY_MILLISECONDS).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    LogError($"Unexpected error.");
                    ConsoleLogger.WriteLine(ex, true, ConsoleColor.Red);
                    if (cacheEntry != null) FailedToSend(cacheEntry);
                    await Task.Delay(WAIT_BEFORE_RETRY_MILLISECONDS).ConfigureAwait(false);
                }
            }
        }

        private void FailedToSend(T cacheEntry, string failureDetail = null)
        {
            var message = "Failed to send" + (string.IsNullOrEmpty(failureDetail) ? "." : ": " + failureDetail);
            LogError(message, cacheEntry);
            cacheEntry.ReadyToSend = false;
            cacheEntry.TotalFailedAttempts++;
            cacheEntry.FailedMessage = failureDetail;
            _cache.Update(cacheEntry);
        }
    }
}