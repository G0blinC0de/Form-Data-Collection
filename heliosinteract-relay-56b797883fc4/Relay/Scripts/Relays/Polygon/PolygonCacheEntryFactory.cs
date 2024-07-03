using System.Collections.Generic;

namespace Helios.Relay.Polygon
{
    public class PolygonCacheEntryFactory : ICacheEntryFactory<PolygonCacheEntry>
    {
        public PolygonCacheEntryFactory(string[] requiredFields)
        {
            _requiredFields = requiredFields;
        }

        private readonly string[] _requiredFields;

        public ValidationResult CreateEntryFromPostData(IPostData postData, out PolygonCacheEntry cacheEntry)
        {
            cacheEntry = null;

            // Confirm experience info was received
            if (postData.Experience == null) return new ValidationResult(false, "No Experience info received.");

            // Confirm required meta fields are present
            var fields = new Dictionary<string, string>();
            var missingFields = new List<string>();
            foreach (var field in _requiredFields)
            {
                if (postData.Experience.meta.TryGetValue(field, out var value))
                {
                    fields[field] = value.ToString();
                }
                else
                {
                    missingFields.Add(field);
                }
            }

            // Invalid if missing required fields
            if (missingFields.Count > 0) return new ValidationResult(false, "Missing required field(s): " + string.Join(", ", missingFields));

            // Success
            cacheEntry = new PolygonCacheEntry(fields, postData.FileInfo);
            return new ValidationResult(true, string.Empty);
        }
    }
}
