using DataViewer.Converters;
using Newtonsoft.Json;

namespace DataViewer.Models
{
    public class TextLine
    {
        [JsonProperty]
        public string Text { get; set; } = "";

        [JsonProperty("Language")]
        [JsonConverter(typeof(LanguageConverter))]
        public Language? Language { get; set; }

        public string TranslatedText { get; set; } = "";

        public Language? TranslationLanguage { get; set; }

        public bool LanguageUnrecognizedOrInconsistent;
    }
}
