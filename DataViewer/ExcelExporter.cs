using DataViewer.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace DataViewer
{
    class ExcelExporter
    {
        public void ExportToExcel(List<LocalizationEntry> entries, string fullpath)
        {
#if DEBUG
            // assertions
            if (entries == null)
                throw new ArgumentNullException("entries", "ExportToExcel should not be called on empty set");
            else if (entries.Count == 0)
                throw new ArgumentException("ExportToExcel should not be called on empty set", "entries");
            if (string.IsNullOrWhiteSpace(fullpath))
                throw new ArgumentNullException("fullpath");
#endif

            var dt = new DataTable("Exported Data");
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Speaker", typeof(string));
            dt.Columns.Add("GUID", typeof(string));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Text", typeof(string));
            dt.Columns.Add("Language", typeof(string));

            for (int i = 0; i < entries.Count; i++)
            {
                DataRow newRow = dt.NewRow();

                LocalizationEntry entry = entries[i];
                newRow[0] = i;
                newRow[1] = entry.Speaker;
                newRow[2] = entry.GUID;

                for (int j = 0; j < entry.Variants.Count; j++)
                {
                    Variant variant = entry.Variants[j];

                    // each consecutive row need to create a new row
                    if (j > 0)
                    {
                        dt.Rows.Add(newRow);
                        newRow = dt.NewRow();
                    }

                    newRow[3] = variant.Name;

                    for (int k = 0; k < variant.TextLines.Count; k++)
                    {
                        TextLine textLine = variant.TextLines[k];

                        // each consecutive row need to create a new row
                        if (k > 0)
                        {
                            dt.Rows.Add(newRow);
                            newRow = dt.NewRow();
                        }

                        newRow[4] = textLine.Text;
                        newRow[5] = textLine.Language;
                    }
                }

                dt.Rows.Add(newRow);
            }

            string sheetName = Path.GetFileName(fullpath);

            byte[] fileContents;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(sheetName);
                worksheet.Cells["A1"].LoadFromDataTable(dt, true);
                fileContents = package.GetAsByteArray();
            }

            File.WriteAllBytes(fullpath, fileContents);
        }
    }
}
