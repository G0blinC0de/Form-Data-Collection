using System;
using Newtonsoft.Json;

namespace Helios.Relay.Dispatch
{
    public class DispatchCacheEntry : CacheEntry
    {
        [JsonConstructor]
        public DispatchCacheEntry() { }

        public DispatchCacheEntry(string name, string email, string phone, RFile fileInfo) : base(true)
        {
            Name = name;
            Email = email;
            Phone = phone;
            FileInfo = fileInfo;
        }

        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public RFile FileInfo { get; private set; }

        public string Uid;
        public string UserToken;
        public string PostId;
        public string SignedPutUrl;
    }
}
