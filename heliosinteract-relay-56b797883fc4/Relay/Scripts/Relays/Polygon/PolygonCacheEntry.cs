using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Helios.Relay.Polygon
{
    public class PolygonCacheEntry : CacheEntry
    {
        [JsonConstructor]
        public PolygonCacheEntry() { }

        public PolygonCacheEntry(Dictionary<string, string> fields, RFile fileInfo) : base(true)
        {
            Fields = fields;
            FileInfo = fileInfo;
        }

        public Dictionary<string, string> Fields { get; private set; }
        public RFile FileInfo { get; private set; }
    }
}
