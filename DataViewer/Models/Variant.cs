using System.Collections.Generic;

namespace DataViewer.Models
{
    public class Variant
    {
        public string Name { get; set; } = "";

        public IList<TextLine> TextLines { get; set; } = new List<TextLine>(0);
    }
}
