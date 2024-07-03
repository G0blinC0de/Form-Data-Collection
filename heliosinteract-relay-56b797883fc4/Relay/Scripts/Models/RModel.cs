namespace Helios.Relay
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public abstract class RModel
    {
        [JsonIgnore] public abstract string endpoint { get; }
        [JsonIgnore] public abstract string logType { get; }
        public virtual string id { get; set; }
        [JsonProperty("metadata")]
        public virtual Dictionary<string, object> meta { get; set; }

        public void AddMetaField(string key, object value)
        {
            if (meta == null)
            {
                meta = new Dictionary<string, object>();
            }
            if (meta.ContainsKey(key))
            {
                if (meta[key].ToString() != value.ToString())
                {
                    ConsoleLogger.WriteLine($"Meta already contains {key} and it does not equal {value}. This is a reserved field. Overwriting {meta[key]} with {value}");
                    meta[key] = value;
                }
            }
            else
            {
                meta.Add(key, value);
            }
        }
    }
}