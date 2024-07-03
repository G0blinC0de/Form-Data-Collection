using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Helios.Relay.GoogleUa
{
    public class GoogleUaCacheEntryValidator : ICacheEntryFactory<GoogleUaCacheEntry>
    {
        private EventTemplate[] _eventTemplates;
        private readonly Dictionary<string, List<string>> _requiredMetaFields = new Dictionary<string, List<string>>()
        {
            ["Experience"] = new List<string>(),
            ["Guest"] = new List<string>(),
            ["File"] = new List<string>(),
        };

        public GoogleUaCacheEntryValidator(IConfiguration configuration)
        {
            _eventTemplates = configuration.GetSection("Google-UA").GetSection("Events").GetChildren()
                .Select(section => new EventTemplate(section)).ToArray();

            for (var i = 0; i < _eventTemplates.Length; i++)
            {
                var result = _eventTemplates[i].Validate(i);
                if (!result.success)
                {
                    ConsoleLogger.WriteLine(result.message, true);
                    return;
                }
                _eventTemplates[i].FindRequiredMetaFields(_requiredMetaFields);
            }
        }

        public ValidationResult CreateEntryFromPostData(IPostData postData, out GoogleUaCacheEntry cacheEntry)
        {
            cacheEntry = null;

            // Find missing keys
            var missingKeys = new Dictionary<string, List<string>>()
            {
                ["Experience"] = new List<string>(),
                ["Guest"] = new List<string>(),
                ["File"] = new List<string>()
            };
            FindMissingKeys(postData.Experience, "Experience");
            FindMissingKeys(postData.Guest, "Guest");
            FindMissingKeys(postData.FileInfo, "File");

            // Return result
            int missingCount = 0;
            foreach (var list in missingKeys.Values) missingCount += list.Count;
            if (missingCount == 0)
            {
                return new ValidationResult(true, "");
            }
            else
            {
                var builder = new StringBuilder("Missing meta keys.");
                AppendMissingKeys("Experience", builder);
                AppendMissingKeys("Guest", builder);
                AppendMissingKeys("File", builder);
                return new ValidationResult(false, builder.ToString());
            }

            void FindMissingKeys(RModel model, string key)
            {
                // If no required meta keys, return
                if (_requiredMetaFields[key].Count == 0) return;

                // If model or modle.meta is null, all keys are missing
                if (model == null || model.meta == null)
                {
                    missingKeys[key].AddRange(_requiredMetaFields[key]);
                    return;
                }

                // Find individual missing keys
                foreach (var field in _requiredMetaFields[key])
                {
                    if (!model.meta.ContainsKey(field)) missingKeys[key].Add(field);
                }
            }

            void AppendMissingKeys(string key, StringBuilder builder)
            {
                if (missingKeys[key].Count > 0) builder.Append($" | {key}: {string.Join(", ", missingKeys[key])}");
            }
        }

        public GoogleUaEvent[] BuidlUaEvents(GoogleUaCacheEntry cacheEntry)
        {
            return _eventTemplates.Select(template => template.BuildEvent(cacheEntry)).ToArray();
        }

        [Serializable]
        private class EventTemplate
        {
            public EventTemplate(IConfigurationSection section)
            {
                category = new DataSource(section.GetSection("category"));
                action = new DataSource(section.GetSection("action"));
                label = new DataSource(section.GetSection("label"));
                value = new DataSource(section.GetSection("value"));
            }

            public DataSource category { get; }
            public DataSource action { get; }
            public DataSource label { get; }
            public DataSource value { get; }

            public GoogleUaEvent BuildEvent(GoogleUaCacheEntry cacheEntry)
            {
                var evCategory = category.GetValue(cacheEntry);
                var evAction = action.GetValue(cacheEntry);
                var googleUaEvent = new GoogleUaEvent(evCategory, evAction);
                if (!label.ignore) googleUaEvent.Label = label.GetValue(cacheEntry);
                if (!value.ignore) googleUaEvent.Value = int.TryParse(value.GetValue(cacheEntry), out var result) ? (int?)result : null;
                return googleUaEvent;
            }

            public void FindRequiredMetaFields(Dictionary<string, List<string>> dictionary)
            {
                FindMetaFieldsInDataSource(category);
                FindMetaFieldsInDataSource(action);
                FindMetaFieldsInDataSource(label);
                FindMetaFieldsInDataSource(value);

                void FindMetaFieldsInDataSource(DataSource dataSource)
                {
                    if (!string.IsNullOrEmpty(dataSource.source))
                    {
                        if (!dictionary[dataSource.source].Contains(dataSource.key)) dictionary[dataSource.source].Add(dataSource.key);
                    }
                }
            }

            public (bool success, string message) Validate(int index)
            {
                // Confirm category and action are not set to ignore
                if (category.ignore) return (false, $"\"category\" field cannot be ignored: Event[{index}].category");
                if (action.ignore) return (false, $"\"action\" field cannot be ignored: Event[{index}].action");

                // Validate DataSources
                var validate = category.Validate(index, "category");
                if (!validate.success) return validate;
                validate = action.Validate(index, "action");
                if (!validate.success) return validate;
                validate = label.Validate(index, "label");
                if (!validate.success) return validate;
                validate = value.Validate(index, "value");
                if (!validate.success) return validate;

                // Valid
                return (true, "");
            }
        }

        [Serializable]
        private struct DataSource
        {
            public string source;
            public string key;
            public string value;
            public bool ignore;

            public DataSource(IConfigurationSection section)
            {
                source = section["source"];
                key = section["key"];
                value = section["value"];
                ignore = section.GetSection("ignore").Get<bool>();
            }

            public string GetValue(GoogleUaCacheEntry cacheEntry)
            {
                if (ignore) return default;

                if (!string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(key))
                {
                    switch (source)
                    {
                        case "Experience":
                            return (string)cacheEntry.Experience.meta[key];

                        case "Guest":
                            return (string)cacheEntry.Guest.meta[key];

                        case "File":
                            return (string)cacheEntry.FileInfo.meta[key];
                    }
                }
                else
                {
                    return value;
                }

                // An error occured
                ConsoleLogger.WriteLine("An error occured while trying to parse a DataSource.");
                return default;
            }

            public (bool success, string message) Validate(int index, string fieldName)
            {
                // Check that the DataSource has some value
                if (source == null && key == null && value == null && ignore == false)
                {
                    return (false, $"Encountered empty or null DataSource: Events[{index}].{fieldName}");
                }

                // Check that the source field has a valid value
                if (source != null)
                {
                    switch (source)
                    {
                        case "Experience":
                        case "Guest":
                        case "File":
                            break;

                        default:
                            return (false, $"Invalid value for Events[{index}].{fieldName}.source: {source}");
                    }
                }

                // Check that source and key always exist as a pair
                if (source != null && key == null) return (false, $"Missing field: Events[{index}].{fieldName}.key");
                if (source == null && key != null) return (false, $"Missing field: Events[{index}].{fieldName}.source");

                // Valid
                return (true, "");
            }

            public override string ToString()
            {
                return $"{{ source: {source}, key: {key}, value: {value}, ignore: {ignore} }}";
            }
        }
    }
}
