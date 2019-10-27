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
                TranslatedText = "";
                TranslationLanguage = null;
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
