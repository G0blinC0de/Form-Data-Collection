using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Helios.Relay.Eshots
{
    public class EshotsCacheEntry : CacheEntry
    {
        [JsonConstructor]
        public EshotsCacheEntry() { }

        public EshotsCacheEntry(string eventTokenId, int rElatId, DateTime created, RFile fileInfo, Dictionary<string, string> data) : base(true)
        {
            EventTokenId = eventTokenId;
            RElatId = rElatId;
            CreateDtmShort = created.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            CreateDtmLong = created.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            FileInfo = fileInfo;
            Data = data;
        }

        public string EventTokenId { get; private set; }
        public int RElatId { get; private set; }
        public string CreateDtmShort { get; private set; }
        public string CreateDtmLong { get; private set; }
        public RFile FileInfo { get; private set; }
        public Dictionary<string, string> Data { get; private set; }
        
        public bool FilePosted { get; set; }
        public bool DataPosted { get; set; }
    }
}
