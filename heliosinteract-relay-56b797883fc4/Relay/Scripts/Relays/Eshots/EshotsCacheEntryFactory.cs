using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Helios.Relay.Eshots
{
    public class EshotsCacheEntryFactory : ICacheEntryFactory<EshotsCacheEntry>
    {
        public EshotsCacheEntryFactory(IConfiguration configuration)
        {
            var dataTransform = configuration.GetSection("Eshots").GetSection("DataTransform");
            _eventTokenIdJsonPath = dataTransform["eventTokenId"];
            _rElatIdJsonPath = dataTransform["rElatId"];
            foreach (var pair in dataTransform.GetSection("answers").AsEnumerable(true))
            {
                _dataJsonPaths[pair.Key] = pair.Value;
            }
        }

        private string _eventTokenIdJsonPath;
        private string _rElatIdJsonPath;
        private Dictionary<string, string> _dataJsonPaths = new Dictionary<string, string>();
        
        public ValidationResult CreateEntryFromPostData(IPostData postData, out EshotsCacheEntry cacheEntry)
        {
            cacheEntry = null;

            var jObject = JObject.FromObject(postData);

            var eventTokenId = jObject.SelectToken(_eventTokenIdJsonPath)?.Value<string>();
            if (string.IsNullOrEmpty(eventTokenId)) return new ValidationResult(false, $"No eventTokenId found at {_eventTokenIdJsonPath}");

            var rElatIdString = jObject.SelectToken(_rElatIdJsonPath)?.Value<string>();
            if (!int.TryParse(rElatIdString, out var rElatId)) return new ValidationResult(false, $"No rElatId found at {_rElatIdJsonPath}");

            DateTime created;
            if (postData.Experience != null && postData.Experience.meta.TryGetValue("createDtm", out var createDtm) && DateTime.TryParse(createDtm.ToString(), out var dateTime))
            {
                created = dateTime;
            }
            else
            {
                created = DateTime.Now;
            }

            Dictionary<string, string> data = null;
            if (_dataJsonPaths.Count > 0)
            {
                data = new Dictionary<string, string>();
                foreach (var entry in _dataJsonPaths)
                {
                    var jProperty = jObject.SelectToken(entry.Value)?.Parent as JProperty;
                    if (jProperty == null) return new ValidationResult(false, $"Missing data field \"{entry.Key}\" at {entry.Value}");
                    var jValue = jProperty.Value as JValue;
                    if (jValue == null) return new ValidationResult(false, $"Value for \"{entry.Key}\" at {entry.Value} must be a value type.");
                    data[entry.Key] = jValue.Value.ToString();
                }
            }

            cacheEntry = new EshotsCacheEntry(eventTokenId, rElatId, created, postData.FileInfo, data);
            return new ValidationResult(true, string.Empty);
        }
    }
}
