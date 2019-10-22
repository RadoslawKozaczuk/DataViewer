﻿using DataViewer.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace DataViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, INotifyPropertyChanging
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region Properties
        LocalizationEntry _selectedEntry;
        public LocalizationEntry SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                if (_selectedEntry != value)
                {
                    NotifyPropertyChanging("SelectedEntry");
                    _selectedEntry = value;
                    NotifyPropertyChanged("SelectedEntry");
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
                    NotifyPropertyChanging("SelectedVariant");
                    _selectedVariant = value;
                    NotifyPropertyChanged("SelectedVariant");
                }
            }
        }
        #endregion

        List<LocalizationEntry> entries;
        GridViewColumnHeader listViewSortCol = null;
        SortAdorner listViewSortAdorner = null;

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            string fullPath;
            var openFileDialog = new OpenFileDialog { Filter = "JSON files (*.json)|*.json" };
            if (openFileDialog.ShowDialog() == true)
                fullPath = openFileDialog.FileName;
            else
                return;

            entries = LocalizationDataDeserializer.DeserializeJsonFile(fullPath);
            EntriesLv.ItemsSource = entries;
            
            // get view
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(EntriesLv.ItemsSource);
            
            // add grouping to the view
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Speaker");
            view.GroupDescriptions.Add(groupDescription);

            // add filtering to the view
            view.Filter = SpeakerFilter;
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
            exporter.ExportToExcel(entries, fullPath);
        }

        void EntriesColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var column = sender as GridViewColumnHeader;
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                EntriesLv.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            EntriesLv.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        bool SpeakerFilter(object item)
        {
            return string.IsNullOrEmpty(txtSpeakerFilter.Text)
                ? true
                : (item as LocalizationEntry).Speaker.IndexOf(txtSpeakerFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        void SpeakerFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(EntriesLv.ItemsSource).Refresh();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify the data context that a data context property is about to change
        protected void NotifyPropertyChanging(string propertyName) => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));

        protected void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        void NameFilterTxt_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextFilterTxt_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
