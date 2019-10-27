using Newtonsoft.Json;
using System;

namespace DataViewer.Converters.JSON
{
    public class LanguageConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) 
            => throw new NotImplementedException("Not implemented yet");

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        => ((string)reader.Value) switch
        {
            "en_us" => Language.English_US,
            "fr_fr" => Language.French,
            "jp_jp" => Language.Japanease,
            _ => (Language?)null
        };

        public override bool CanConvert(Type objectType) => objectType == typeof(string);
    }
}