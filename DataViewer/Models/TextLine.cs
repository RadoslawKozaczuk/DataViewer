using DataViewer.Converters.JSON;
using Newtonsoft.Json;

namespace DataViewer.Models
{
    public class TextLine
    {
        string _text = "";

        [JsonProperty]
        public string Text 
        {
            get => _text;
            set
            {
                _text = value;
                TranslatedText = null;
                TranslationLanguage = null; // notify property change should be fired here (or in VM)
            } 
        }

        [JsonProperty("Language")]
        [JsonConverter(typeof(LanguageConverter))]
        public Language? Language { get; set; }

        public string TranslatedText { get; set; } = "";

        public Language? TranslationLanguage { get; set; }

        public string TranslationLanguageShortId 
            => TranslationLanguage.HasValue 
            ? TranslationLanguage.Value.ToGoogleLangId() 
            : "";

        public bool LanguageUnrecognizedOrInconsistent;
    }
}
