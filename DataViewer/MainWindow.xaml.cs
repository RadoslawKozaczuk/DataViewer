using DataViewer.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace DataViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties
        LocalizationEntry _selectedEntry;
        public LocalizationEntry SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                if (_selectedEntry != value)
                {
                    _selectedEntry = value;

                    // auto-select quality-of-life improvement
                    // if there is only one variant to select from (after applying filtering) it will be automatically selected
                    List<Variant> variants = _selectedEntry.Variants.Where(v => VariantFilter(v)).ToList();
                    if (variants.Count == 1)
                        SelectedVariant = variants[0];

                    NotifyPropertyChanged("SelectedEntry");
                    var view = (CollectionView)CollectionViewSource.GetDefaultView(VariantsDataGrid.ItemsSource);
                    if (view != null)
                        view.Filter = VariantFilter;
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
                    _selectedVariant = value;
                    NotifyPropertyChanged("SelectedVariant");
                    var view = (CollectionView)CollectionViewSource.GetDefaultView(TextLinesDataGrid.ItemsSource);
                    if(view != null)
                        view.Filter = TextLineFilter;
                }
            }
        }
        #endregion

        List<LocalizationEntry> LoadedData;
        GridViewColumnHeader listViewSortCol = null;
        SortAdorner listViewSortAdorner = null;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            string fullPath;
            var openFileDialog = new OpenFileDialog { Filter = "JSON files (*.json)|*.json" };
            if (openFileDialog.ShowDialog() == true)
                fullPath = openFileDialog.FileName;
            else
                return;

            LoadedData = LocalizationDataDeserializer.DeserializeJsonFile(fullPath);
            EntriesDataGrid.ItemsSource = LoadedData;

            var view = (CollectionView)CollectionViewSource.GetDefaultView(EntriesDataGrid.ItemsSource);
            view.Filter = EntryFilter;

            var groupDescription = new PropertyGroupDescription("Speaker");
            view.GroupDescriptions.Add(groupDescription);
        }

        void ExportToExcelButton_Click(object sender, RoutedEventArgs e)
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
            exporter.ExportToExcel(LoadedData, fullPath);
        }

        void EntriesColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var column = sender as GridViewColumnHeader;
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                EntriesDataGrid.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            EntriesDataGrid.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        bool EntryFilter(object item)
        {
            if(!string.IsNullOrEmpty(SpeakerFilterTxt.Text))
            {
                if ((item as LocalizationEntry).Speaker.IndexOf(SpeakerFilterTxt.Text, StringComparison.OrdinalIgnoreCase) < 0)
                    return false;
            }

            if (!string.IsNullOrEmpty(GUIDFilterTxt.Text))
            {
                if ((item as LocalizationEntry).GUID.IndexOf(GUIDFilterTxt.Text, StringComparison.OrdinalIgnoreCase) < 0)
                    return false;
            }

            return true;
        }

        bool VariantFilter(object item)
        {
            if (!string.IsNullOrEmpty(NameFilterTxt.Text))
            {
                if ((item as Variant).Name.IndexOf(NameFilterTxt.Text, StringComparison.OrdinalIgnoreCase) < 0)
                    return false;
            }

            return true;
        }

        bool TextLineFilter(object item)
        {
            if (!string.IsNullOrEmpty(TextFilterTxt.Text))
            {
                if ((item as TextLine).Text.IndexOf(TextFilterTxt.Text, StringComparison.OrdinalIgnoreCase) < 0)
                    return false;
            }

            //if (!string.IsNullOrEmpty(LanguageFilterTxt.Text))
            //{
            //    if ((item as TextLine).Language.IndexOf(LanguageFilterTxt.Text, StringComparison.OrdinalIgnoreCase) < 0)
            //        return false;
            //}

            return true;
        }

        void EntryFilter_ValueChanged(object sender, TextChangedEventArgs e) => CollectionViewSource.GetDefaultView(EntriesDataGrid.ItemsSource).Refresh();

        void VariantFilter_ValueChanged(object sender, TextChangedEventArgs e) => CollectionViewSource.GetDefaultView(VariantsDataGrid.ItemsSource).Refresh();

        void TextLineFilter_ValueChanged(object sender, TextChangedEventArgs e) => CollectionViewSource.GetDefaultView(TextLinesDataGrid.ItemsSource).Refresh();

        protected void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        void EntriesDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

        }

        void VariantsDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

        }

        void TextLinesDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

        }
    }
}
