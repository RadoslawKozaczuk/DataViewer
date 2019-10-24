using Caliburn.Micro;
using DataViewer.Commands;
using DataViewer.Models;
using Google.Cloud.Translation.V2;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DataViewer.ViewModels
{
    public class MainViewModel : PropertyChangedBase
    {
        #region Properties

        // Grouping disables virtualization. This can bring huge performance issues on large data sets. 
        // So be careful when using it.
        List<LocalizationEntry> _entries;
        public List<LocalizationEntry> Entries
        {
            get => _entries;
            set => Set(ref _entries, value);
        }

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

                        _variantsView = (ListCollectionView)CollectionViewSource.GetDefaultView(SelectedEntry.Variants);
                        _variantsView.Filter = VariantFilter;
                    }
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

                    if(_selectedVariant != null)
                    {
                        _textLinesView = (ListCollectionView)CollectionViewSource.GetDefaultView(SelectedVariant.TextLines);
                        _textLinesView.Filter = TextLineFilter;
                    }
                }
            }
        }

        TextLine _selectedTextLine;
        public TextLine SelectedTextLine
        {
            get => _selectedTextLine;
            set
            {
                Set(ref _selectedTextLine, value); // this one-liner does both set and notify
                NotifyOfPropertyChange(() => CanTranslate);
            }
        }

        string _speakerFilter;
        public string SpeakerFilter 
        {
            get => _speakerFilter;
            set
            {
                _speakerFilter = value;
                _entriesView?.ForceCommitRefresh();
            }    
        }

        string _guidFilter;
        public string GUIDFilter
        {
            get => _guidFilter;
            set
            {
                _guidFilter = value;
                _entriesView?.ForceCommitRefresh();
            }
        }

        string _nameFilter;
        public string NameFilter
        {
            get => _nameFilter;
            set
            {
                _nameFilter = value;
                _variantsView?.ForceCommitRefresh();
            }
        }

        string _textFilter;
        public string TextFilter
        {
            get => _textFilter;
            set
            {
                _textFilter = value;
                _textLinesView?.ForceCommitRefresh();
            }
        }

        Language _translationLanguage;
        public Language TranslationLanguage 
        {
            get => _translationLanguage;
            set
            {
                _translationLanguage = value;
                NotifyOfPropertyChange(() => CanTranslate);
            }
        }
        #endregion

        ListCollectionView _entriesView;
        ListCollectionView _variantsView;
        ListCollectionView _textLinesView;

        public MainViewModel() : base()
        {
            // for convenience we pass notifiers to command stack so whenever an operation is executed on it, notifiers will also be called
            CommandStack.NotifyUndoAction = () => NotifyOfPropertyChange(() => CanUndo);
            CommandStack.NotifyRedoAction = () => NotifyOfPropertyChange(() => CanRedo);
        }

        #region Button methods
        public void OpenFile()
        {
            string fullPath;
            var openFileDialog = new OpenFileDialog { Filter = "JSON files (*.json)|*.json" };
            if (openFileDialog.ShowDialog() == true)
                fullPath = openFileDialog.FileName;
            else
                return;

            List<LocalizationEntry> entries;

            try
            {
                using StreamReader file = File.OpenText(fullPath);
                var serializer = new JsonSerializer();
                entries = (List<LocalizationEntry>)serializer.Deserialize(file, typeof(List<LocalizationEntry>));
            }
            catch (Exception)
            {
                MessageBox.Show("File is corrupted and impossible to read.", "File corrupted", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (entries == null || entries.Count == 0)
                return;

            Entries = entries;

            _entriesView = (ListCollectionView)CollectionViewSource.GetDefaultView(Entries);
            _entriesView.GroupDescriptions.Add(new PropertyGroupDescription("Speaker"));
            _entriesView.Filter = EntryFilter;
            
            NotifyOfPropertyChange(() => CanExportToExcel);
        }

        public bool CanExportToExcel => Entries != null && Entries.Count != 0;

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

        public bool CanTranslate => SelectedTextLine != null && SelectedTextLine.Language != TranslationLanguage;

        /// <summary>
        /// Calls the Google Translation Cloud to perform text translation.
        /// </summary>
        public void Translate()
        {
            TranslationResult response;
            try
            {
                using TranslationClient client = TranslationClient.Create();
                response = client.TranslateText(
                    text: SelectedTextLine.Text,
                    targetLanguage: TranslationLanguage.ToGoogleLangId(),
                    sourceLanguage: SelectedTextLine.Language.ToGoogleLangId(),
                    model: TranslationModel.NeuralMachineTranslation);
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Translation error. No Internet connection or Google Translation Cloud Service is inactive.", 
                    "Translation Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);

                return;
            }

            SelectedTextLine.TranslatedText = response.TranslatedText;
            SelectedTextLine.TranslationLanguage = TranslationLanguage;

            _textLinesView.ForceCommitRefresh();
        }

        public bool CanUndo => CommandStack.UndoCount > 0;

        public void Undo()
        {
            CommandStack.Undo();
            RefreshAllViews();
        }

        public bool CanRedo => CommandStack.RedoCount > 0;

        public void Redo()
        {
            CommandStack.Redo();
            RefreshAllViews();
        }

        void RefreshAllViews()
        {
            _entriesView?.ForceCommitRefresh();
            _variantsView?.ForceCommitRefresh();
            _textLinesView?.ForceCommitRefresh();
        }
        #endregion

        public void Entries_CellEditEnding(DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
                return;

            if (e.Column.Header.ToString() == "Speaker")
            {
                var undoCmd = new UndoRedoCommand(
                    objRef: SelectedEntry, 
                    oldValue: new LocalizationEntry { Speaker = SelectedEntry.Speaker },
                    newValue: new LocalizationEntry { Speaker = ((TextBox)e.EditingElement).Text });

                CommandStack.Push(undoCmd);
            }
        }

        public void Variants_CellEditEnding(DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
                return;

            if (e.Column.Header.ToString() == "Name")
            {
                var undoCmd = new UndoRedoCommand(
                    objRef: SelectedVariant, 
                    oldValue: new Variant { Name = SelectedVariant.Name },
                    newValue: new Variant { Name = ((TextBox)e.EditingElement).Text });

                CommandStack.Push(undoCmd);
            }
        }

        // doesnt work for now something is wrong with the xaml structure
        public void TextLines_CellEditEnding(DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
                return;

            if (e.Column.Header.ToString() == "Text")
            {
                var undoCmd = new UndoRedoCommand(
                    objRef: SelectedTextLine, 
                    oldValue: new TextLine { Text = SelectedTextLine.Text },
                    newValue: new TextLine { Text = SelectedTextLine.Text });

                CommandStack.Push(undoCmd);
            }
        }

        #region Filters
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
            => string.IsNullOrWhiteSpace(TextFilter) 
            || (item as TextLine).Text.IndexOf(TextFilter, StringComparison.OrdinalIgnoreCase) >= 0;
        #endregion
    }
}
