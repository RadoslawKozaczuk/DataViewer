using DataViewer.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace DataViewer
{
    static class LocalizationDataDeserializer
    {
        public static List<LocalizationEntry> DeserializeJsonFile(string fullpath)
        {
            using StreamReader file = File.OpenText(fullpath);
            var serializer = new JsonSerializer();
            return (List<LocalizationEntry>)serializer.Deserialize(file, typeof(List<LocalizationEntry>));
        }
    }
}
