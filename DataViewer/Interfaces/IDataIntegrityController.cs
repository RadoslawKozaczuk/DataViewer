using DataViewer.Models;
using System.Collections.Generic;

namespace DataViewer.Interfaces
{
    public interface IDataIntegrityController
    {
        void HealDocument(IList<LocalizationEntry> entries);
        bool PerformFullScan(IList<LocalizationEntry> entries);
        bool ScanLocalizationEntry(LocalizationEntry entry);
        bool ScanVariant(Variant variant);
        bool ScanTextLine(TextLine textLine);
    }
}
