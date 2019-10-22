using Newtonsoft.Json;
using System;

namespace DataViewer.Converters
{
    public class LanguageConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            switch ((Language)value)
            {
                case Language.English_US:
                    writer.WriteValue("en_us");
                    break;
                case Language.French:
                    writer.WriteValue("fr_fr");
                    break;
                case Language.Japanease:
                    writer.WriteValue("jp_jp");
                    break;
                default:
                    throw new ArgumentException("Not supported language.");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch ((string)reader.Value)
            {
                case "en_us": return Language.English_US;
                case "fr_fr": return Language.French;
                case "jp_jp": return Language.Japanease;
                default: throw new ArgumentException("Not supported language."); ;
            }
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(string);
    }
}
