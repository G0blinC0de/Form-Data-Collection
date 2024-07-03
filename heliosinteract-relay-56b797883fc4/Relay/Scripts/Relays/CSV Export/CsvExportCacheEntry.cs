using System.Collections.Generic;
using Newtonsoft.Json;

namespace Helios.Relay.CsvExport
{
    public class CsvExportCacheEntry : CacheEntry
    {
        [JsonConstructor]
        public CsvExportCacheEntry() { }

        public CsvExportCacheEntry(Dictionary<string, string> values) : base(true)
        {
            Values = values;
        }

        public Dictionary<string, string> Values { get; private set; }
    }
}
