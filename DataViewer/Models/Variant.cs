using System.Collections.Generic;

namespace DataViewer.Models
{
    public class Variant
    {
        public string Name { get; set; }

        public List<TextLine> TextLines { get; set; }

        public Variant(string name)
        {
            Name = name;
            TextLines = new List<TextLine>();
        }
    }
}
