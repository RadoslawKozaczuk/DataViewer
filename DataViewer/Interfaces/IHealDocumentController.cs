using DataViewer.Models;
using System.Collections.Generic;

namespace DataViewer.Interfaces
{
    public interface IHealDocumentController
    {
        string Translate(string text, Language source, Language target);
        void HealDocument(IList<LocalizationEntry> entries);
        bool PerformFullScan(IList<LocalizationEntry> entries);
    }
}
