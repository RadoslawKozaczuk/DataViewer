using DataViewer.Models;
using System.Collections.Generic;

namespace DataViewer.Interfaces
{
    public interface IDataIntegrityController
    {
        /// <summary>
        /// Returns false when data is invalid, and true when data is valid.
        /// </summary>
        bool PerformFullScan(IList<LocalizationEntry> entries);
        bool HealDocument(IList<LocalizationEntry> entries);
    }
}
