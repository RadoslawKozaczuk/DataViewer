using DataViewer.Models;
using System.Collections.Generic;

namespace DataViewer.Interfaces
{
    public interface IDataIntegrityController
    {
        /// <summary>
        /// Returns null when scan could not be completed, false when data is invalid, and true when data is valid.
        /// </summary>
        bool? PerformFullScan(IList<LocalizationEntry> entries);
        bool ScanLocalizationEntry(LocalizationEntry entry);
        bool ScanVariant(Variant variant);
        bool ScanTextLine(TextLine textLine);
        bool HealDocument(IList<LocalizationEntry> entries);
    }
}
