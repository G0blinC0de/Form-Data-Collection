using System;
using LiteDB;
using Newtonsoft.Json;

namespace Helios.Relay
{
    public abstract class CacheEntry
    {
        [JsonConstructor]
        protected CacheEntry() { }

        protected CacheEntry(bool initialize)
        {
            if (initialize)
            {
                EntryId = UniqueId.Create();
                TimeCreated = DateTime.UtcNow.ToString("o");
                ReadyToSend = true;
            }
        }

        [BsonId] public string EntryId { get; protected set; }
        public string TimeCreated { get; protected set; }
        public bool ReadyToSend { get; set; }
        public bool IsFrozen { get; set; }
        public int TotalFailedAttempts { get; set; }
        public string FailedMessage { get; set; }
    }
}