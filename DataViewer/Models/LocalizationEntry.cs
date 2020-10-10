using DataViewer.Converters.JSON;
using DataViewer.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DataViewer.Models
{
    public class LocalizationEntry : IModel
    {
        [JsonProperty("Speaker")]
        public string Speaker
        {
            get => _speaker;
            set
            {
                _speaker = value;
                SpeakerIsValid = true;
            }
        }
        string _speaker = "";

        [JsonProperty("GUID")]
        public string GUID
        {
            get => _guid;
            set
            {
                _guid = value;
                GUIDIsValid = true;
            }
        }
        string _guid = "";

        [JsonConverter(typeof(VariantConverter))]
        public IList<Variant> Variants { get; set; } = new List<Variant>(0);

        [JsonIgnore]
        public bool SpeakerIsValid { get; set; } = true;

        [JsonIgnore]
        public bool GUIDIsValid { get; set; } = true;

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
