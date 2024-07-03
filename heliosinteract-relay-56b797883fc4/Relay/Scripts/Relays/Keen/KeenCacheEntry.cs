using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Helios.Relay.Keen
{
    public class KeenCacheEntry : CacheEntry
    {
        [JsonConstructor]
        public KeenCacheEntry() { }
        public KeenCacheEntry(IPostData postData) : base(true)
        {
            var data = new
            {
                keen = new { timestamp = TimeCreated },
                experience = postData.Experience,
                guest = postData.Guest,
                file = postData.FileInfo
            };
            Data = JsonConvert.SerializeObject(data, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
        }

        public string Data { get; private set; }
    }
}
