using DataViewer.Converters.JSON;
using DataViewer.Interfaces;
using Newtonsoft.Json;

namespace DataViewer.Models
{
    public class TextLine : IModel
    {
        [JsonProperty]
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                TranslatedText = "";
                TranslationLanguage = null;
                TextIsValid = true;
            }
        }
        string _text = "";

        [JsonIgnore]
        public bool TextIsValid { get; set; } = true;

        [JsonProperty("Language")]
        [JsonConverter(typeof(LanguageConverter))]
        public Language? Language 
        {
            get => _language;
            set
            {
                _language = value;
                LanguageIsValid = true;
            }
        }
        Language? _language;

        [JsonIgnore]
        public bool LanguageIsValid { get; set; } = true;

        [JsonIgnore]
        public string TranslatedText { get; set; } = "";

        [JsonIgnore]
        public Language? TranslationLanguage { get; set; }

        [JsonIgnore]
        public string TranslationLanguageShortId => TranslationLanguage.HasValue ? TranslationLanguage.Value.ToGoogleLangId() : "";

        public TextLine() { }

        public TextLine(TextLine copy)
        {
            Text = copy.Text;
            Language = copy.Language;
            TranslatedText = copy.TranslatedText;
            TranslationLanguage = copy.TranslationLanguage;
        }
    }
}
