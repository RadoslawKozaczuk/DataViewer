using Newtonsoft.Json;
using System.Collections.Generic;

namespace DataViewer.Models
{
    public class LocalizationEntry
    {
        [JsonProperty("Speaker")]
        public string Speaker { get; set; } = "";

        [JsonProperty("GUID")]
        public string GUID { get; set; } = "";

        [JsonConverter(typeof(VariantConverter))]
        public IList<Variant> Variants { get; set; }

        public override string ToString() => $"Speaker {Speaker} GUID:{GUID}";
    }
}
