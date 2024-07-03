using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Helios.Relay
{
    public class CustomJsonConverter : CustomCreationConverter<IDictionary<string, object>>
    {
        public override IDictionary<string, object> Create(Type objectType)
        {
            return new Dictionary<string, object>();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ToDictionary(JObject.Load(reader));
        }

        private static IDictionary<string, object> ToDictionary(JObject jObject)
        {
            var result = new Dictionary<string, object>();
            foreach (var item in jObject)
            {
                object value;
                switch (item.Value)
                {
                    case JArray a:
                        value = ToList(a);
                        break;

                    case JObject o:
                        value = ToDictionary(o);
                        break;

                    case JValue v:
                        value = v.Value;
                        break;

                    default:
                        throw new JsonSerializationException();
                }
                result.Add(item.Key, value);
            }
            return result;
        }

        private static IList<object> ToList(JArray jArray)
        {
            var result = new List<object>();
            foreach (var item in jArray)
            {
                object value;
                switch (item)
                {
                    case JArray a:
                        value = ToList(a);
                        break;

                    case JObject o:
                        value = ToDictionary(o);
                        break;

                    case JValue v:
                        value = v.Value;
                        break;

                    default:
                        throw new JsonSerializationException();
                }
                result.Add(value);
            }
            return result;
        }
    }
}