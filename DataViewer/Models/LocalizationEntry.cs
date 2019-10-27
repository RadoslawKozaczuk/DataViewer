using DataViewer.Converters.JSON;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DataViewer.Models
{
    public class LocalizationEntry
    {
        string _speaker = "";
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

        string _guid = "";
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
