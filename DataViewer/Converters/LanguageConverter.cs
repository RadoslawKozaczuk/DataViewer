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
                    throw new ArgumentException("Not supported language. Please extend LanguageConverter functionality.");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        => ((string)reader.Value) switch
        {
            "en_us" => Language.English_US,
            "fr_fr" => Language.French,
            "jp_jp" => Language.Japanease,
            _ => throw new ArgumentException("Not supported language. Please extend LanguageConverter functionality.")
        };

        public override bool CanConvert(Type objectType) => objectType == typeof(string);
    }
}
