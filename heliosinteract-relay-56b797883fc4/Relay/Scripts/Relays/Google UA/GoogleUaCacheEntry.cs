using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Helios.Relay.GoogleUa
{
    public class GoogleUaCacheEntry : CacheEntry
    {
        public List<GoogleUaEvent> Events { get; set; } = new List<GoogleUaEvent>();

        [JsonConstructor]
        public GoogleUaCacheEntry() { }
        public GoogleUaCacheEntry(IPostData data) : base(true)
        {
            Experience = data.Experience;
            Guest = data.Guest;
            FileInfo = data.FileInfo;
        }

        public RExperience Experience { get; private set; }
        public RGuest Guest { get; private set; }
        public RFile FileInfo { get; private set; }
    }
}
