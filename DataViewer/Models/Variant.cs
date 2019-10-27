using Newtonsoft.Json;
using System.Collections.Generic;

namespace DataViewer.Models
{
    public class Variant
    {
        [JsonProperty("Name")]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NameIsValid = true;
            }
        }
        string _name = "";

        [JsonIgnore]
        public bool NameIsValid { get; set; } = true;

        public IList<TextLine> TextLines { get; set; } = new List<TextLine>(0);
    }
}
