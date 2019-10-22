﻿using DataViewer.Converters;
using Newtonsoft.Json;

namespace DataViewer.Models
{
    class TextLine
    {
        [JsonProperty]
        public string Text { get; set; }

        [JsonProperty("Language")]
        [JsonConverter(typeof(LanguageConverter))]
        public Language Language { get; set; }
    }
}
