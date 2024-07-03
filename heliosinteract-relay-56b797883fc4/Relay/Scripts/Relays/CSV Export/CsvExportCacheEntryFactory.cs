using System;
using System.Collections.Generic;

namespace Helios.Relay.CsvExport
{
    public class CsvExportCacheEntryFactory : ICacheEntryFactory<CsvExportCacheEntry>
    {
        public CsvExportCacheEntryFactory(CsvExportField[] fields)
        {
            _fields = fields;
        }

        private CsvExportField[] _fields;

        public ValidationResult CreateEntryFromPostData(IPostData postData, out CsvExportCacheEntry cacheEntry)
        {
            cacheEntry = null;
            var values = new Dictionary<string, string>();

            foreach (var field in _fields)
            {
                Dictionary<string, object> meta;
                switch (field.Source)
                {
                    case "Experience":
                        meta = postData.Experience?.meta;
                        break;

                    case "Guest":
                        meta = postData.Guest?.meta;
                        break;

                    case "File":
                        meta = postData.FileInfo?.meta;
                        break;

                    default:
                        // Unreachable
                        throw new RelayException($"Internal error: Invalid value for source field \"{field.Source}\"");
                }

                // If the field is missing from the data, output an empty string
                values[$"{field.Source}.{field.Key}"] = (meta != null && meta.TryGetValue(field.Key, out var value)) ? value.ToString() : string.Empty;
            }

            cacheEntry = new CsvExportCacheEntry(values);
            return new ValidationResult(true, string.Empty);
        }
    }
}
