using Caliburn.Micro;
using DataViewer.Models;
using Google.Cloud.Translation.V2;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace DataViewer.ViewModels
{
    public class MainViewModel : PropertyChangedBase
    {
        #region Properties

        //        // Grouping disables virtualization! This can bring huge performance issues on large data sets. 
        //        // So be careful when using it.



        LocalizationEntry _selectedEntry;
        public LocalizationEntry SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                if (_selectedEntry != value)
                {
                    Set(ref _selectedEntry, value);

                    // auto-select quality-of-life improvement
                    // if there is only one variant to select from (after applying filtering) it will be automatically selected
                    if(_selectedEntry != null)
                    {
                        List<Variant> variants = _selectedEntry.Variants.Where(v => VariantFilter(v)).ToList();
                        if (variants.Count == 1)
                            SelectedVariant = variants[0];
                    }

                    _variantsView = CollectionViewSource.GetDefaultView(SelectedEntry.Variants);
                    _variantsView.Filter = VariantFilter;
                }
            }
        }

        Variant _selectedVariant;
        public Variant SelectedVariant
        {
            get => _selectedVariant;
            set
            {
                if (_selectedVariant != value)
                {
                    Set(ref _selectedVariant, value); // this one-liner does both set and notify

                    _textLinesView = CollectionViewSource.GetDefaultView(SelectedVariant.TextLines);
                    _textLinesView.Filter = TextLineFilter;
                }
            }
        }

        string _speakerFilter;
        public string SpeakerFilter 
        {
            get => _speakerFilter;
            set
            {
                _speakerFilter = value;
                _entriesView.Refresh();
            }    
        }

        string _guidFilter;
        public string GUIDFilter
        {
            get => _guidFilter;
            set
            {
                _guidFilter = value;
                _entriesView.Refresh();
            }
        }

        string _nameFilter;
        public string NameFilter
        {
            get => _nameFilter;
            set
            {
                _nameFilter = value;
                _variantsView.Refresh();
            }
        }

        string _textFilter;
        public string TextFilter
        {
            get => _textFilter;
            set
            {
                _textFilter = value;
                _textLinesView.Refresh();
            }
        }

        List<LocalizationEntry> _entries;
        public List<LocalizationEntry> Entries
        {
            get => _entries;
            set => Set(ref _entries, value);
        }
        #endregion

        GridViewColumnHeader _listViewSortCol;
        SortAdorner _listViewSortAdorner;
        ICollectionView _entriesView;
        ICollectionView _variantsView;
        ICollectionView _textLinesView;

        public void OpenFile()
        {
            string fullPath;
            var openFileDialog = new OpenFileDialog { Filter = "JSON files (*.json)|*.json" };
            if (openFileDialog.ShowDialog() == true)
                fullPath = openFileDialog.FileName;
            else
                return;

            Entries = LocalizationDataDeserializer.DeserializeJsonFile(fullPath);

            _entriesView = CollectionViewSource.GetDefaultView(Entries);
            _entriesView.GroupDescriptions.Add(new PropertyGroupDescription("Speaker"));
            _entriesView.Filter = EntryFilter;
        }

        public bool CanExportToExcel() => Entries != null && Entries.Count != 0;

        public void ExportToExcel()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                AddExtension = true,
                DefaultExt = "xlsx",
                FileName = "exportedData"
            };

            string fullPath;
            if (saveFileDialog.ShowDialog() == true)
                fullPath = saveFileDialog.FileName;
            else
                return;

            var exporter = new ExcelExporter();
            exporter.ExportToExcel(Entries, fullPath);
        }

        void EntriesColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var column = sender as GridViewColumnHeader;
            string sortBy = column.Tag.ToString();
            if (_listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(_listViewSortCol).Remove(_listViewSortAdorner);
                _entriesView.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (_listViewSortCol == column && _listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            _listViewSortCol = column;
            _listViewSortAdorner = new SortAdorner(_listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(_listViewSortCol).Add(_listViewSortAdorner);
            _entriesView.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        void EntriesDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

        }

        void VariantsDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

        }

        void TextLinesDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

        }

        public bool CanUndo()
        {
            return true;
        }

        public void Undo()
        {
            using TranslationClient client = TranslationClient.Create();
            var response = client.TranslateText(
                text: "Hello World.",
                targetLanguage: "pl",  // Polish
                sourceLanguage: "en",  // English
                model: TranslationModel.NeuralMachineTranslation);
            Console.WriteLine(response.TranslatedText);

            MessageBox.Show($"Hello World. => {response.TranslatedText}");
        }

        public bool CanRedo()
        {
            return true;
        }

        public void Redo()
        {
            MessageBox.Show("Redo Clicked");
        }

        bool EntryFilter(object item)
        {
            var entry = item as LocalizationEntry;

            if (!string.IsNullOrWhiteSpace(SpeakerFilter) 
                && entry.Speaker.IndexOf(SpeakerFilter, StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            if (!string.IsNullOrWhiteSpace(GUIDFilter) 
                && entry.GUID.IndexOf(GUIDFilter, StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            return true;
        }

        bool VariantFilter(object item) 
            => string.IsNullOrWhiteSpace(NameFilter) 
            || (item as Variant).Name.IndexOf(NameFilter, StringComparison.OrdinalIgnoreCase) >= 0;

        bool TextLineFilter(object item)
        {
            var textLine = item as TextLine;

            if (!string.IsNullOrWhiteSpace(TextFilter) && textLine.Text.IndexOf(TextFilter, StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            //if (!string.IsNullOrEmpty(LanguageFilterTxt.Text))
            //{
            //    if ((item as TextLine).Language.IndexOf(LanguageFilterTxt.Text, StringComparison.OrdinalIgnoreCase) < 0)
            //        return false;
            //}

            return true;
        }
    }
}
