using DataViewer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace DataViewer
{
    public class VariantConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) 
            => throw new NotImplementedException("Not implemented yet");

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return new List<Variant>(0);

            JObject obj = JObject.Load(reader);
            var varList = new List<Variant>(obj.Count);

            foreach(KeyValuePair<string, JToken> kvp in obj)
            {
                var variant = new Variant { Name = kvp.Key };

                // add text lines
                foreach (JToken token in kvp.Value)
                    variant.TextLines.Add(token.ToObject<TextLine>());

                varList.Add(variant);
            }

            return varList;
        }

        public override bool CanWrite { get => false; }

        public override bool CanConvert(Type objectType) => false;
    }
}
