using System;
using System.Text;
using Newtonsoft.Json;

namespace Helios.Relay.GoogleUa
{
    public class GoogleUaEvent
    {
        [JsonConstructor]
        public GoogleUaEvent() { }
        public GoogleUaEvent(string category, string action, string label = null, int? value = null)
        {
            Category = category;
            Action = action;
            Label = label;
            Value = value;
        }

        public string Category { get; set; }
        public string Action { get; set; }
        public string Label { get; set; }
        public int? Value { get; set; }

        public string UrlParameters()
        {
            var builder = new StringBuilder();
            builder.Append("&t=event");
            builder.Append($"&ec={Category}");
            builder.Append($"&ea={Action}");
            if (!string.IsNullOrWhiteSpace(Label)) builder.Append($"&el={Label}");
            if (Value.HasValue) builder.Append($"&ev={Value}");
            return builder.ToString();
        }

        public override string ToString()
        {
            return $"{{ Category: {Category}, Action: {Action}, Label: {Label}, Value: {Value} }}";
        }
    }
}
