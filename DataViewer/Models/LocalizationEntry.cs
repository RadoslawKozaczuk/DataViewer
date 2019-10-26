using DataViewer.Converters.JSON;
using Newtonsoft.Json;
using System;
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
        public List<Variant> Variants { get; set; } = new List<Variant>(0);

        public override string ToString() => $"Speaker {Speaker} GUID:{GUID}";

        public LocalizationEntry()
        {
            GUID = Guid.NewGuid().ToPlainUpper();
        }

        public LocalizationEntry(string speaker)
        {
            Speaker = speaker;
            GUID = Guid.NewGuid().ToPlainUpper();
        }

        public LocalizationEntry(string speaker, string guid)
        {
            Speaker = speaker;
            GUID = guid;
        }
    }
}
