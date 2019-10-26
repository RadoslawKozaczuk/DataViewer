using DataViewer.Input;
using DataViewer.Interfaces;
using DataViewer.Models;
using DataViewer.UndoRedoCommands;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DataViewer.ViewModels
{
    public class ShellViewModel : ViewModelBase, IShellViewModel
    {
        #region Properties
        List<LocalizationEntry> _entries;
        public List<LocalizationEntry> Entries
        {
            get => _entries;
            set
            {
                Set(ref _entries, value);
                NotifyOfPropertyChange(() => CanAddEntry);
            }
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
                    if (_selectedEntry != null)
                    {
                        List<Variant> variants = _selectedEntry.Variants.Where(v => VariantFilter(v)).ToList();
                        if (variants.Count == 1)
                            SelectedVariant = variants[0];

                        _variantsView = (ListCollectionView)CollectionViewSource.GetDefaultView(SelectedEntry.Variants);
                        _variantsView.Filter = VariantFilter;

                        NotifyOfPropertyChange(() => CanAddVariant);
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

                    if (_selectedVariant != null)
                    {
                        _textLinesView = (ListCollectionView)CollectionViewSource.GetDefaultView(SelectedVariant.TextLines);
                        _textLinesView.Filter = TextLineFilter;
                        NotifyOfPropertyChange(() => CanAddTextLine);
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

        bool _isTranslating;
        public bool IsTranslating
        {
            get
            {
                return _isTranslating;
            }
            set
            {
                Set(ref _isTranslating, value);
            }
        }
        #endregion

        readonly IHealDocumentController _healDocumentController;
        readonly CommandController<IUndoRedoCommand> _commandController;

        ListCollectionView _entriesView;
        ListCollectionView _variantsView;
        ListCollectionView _textLinesView;

        bool _dataInconsistencyDetected;

        public ShellViewModel(IHealDocumentController healDocumentController)
            : base()
        {
            _healDocumentController = healDocumentController;

            // for convenience we pass notifiers to command stack so whenever an operation is executed on it, notifiers will also be called
            _commandController = new CommandController<IUndoRedoCommand>(
                notifyUndoAction: () => NotifyOfPropertyChange(() => CanUndo),
                notifyRedoAction: () => NotifyOfPropertyChange(() => CanRedo));
        }

        protected override IEnumerable<InputBindingCommand> GetInputBindingCommands()
        {
            yield return new InputBindingCommand(OpenFile)
            {
                GestureModifier = ModifierKeys.Control,
                GestureKey = Key.O
            };

            yield return new InputBindingCommand(ExportToExcel)
            {
                GestureModifier = ModifierKeys.Control,
                GestureKey = Key.E
            }.If(() => CanExportToExcel);

            yield return new InputBindingCommand(Undo)
            {
                GestureModifier = ModifierKeys.Control,
                GestureKey = Key.Z
            }.If(() => CanUndo);

            yield return new InputBindingCommand(Redo)
            {
                GestureModifier = ModifierKeys.Control,
                GestureKey = Key.R
            }.If(() => CanRedo);
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
            _entriesView.Filter = EntryFilter;

            // Grouping disables virtualization. This can bring huge performance issues on large data sets.
            _entriesView.GroupDescriptions.Add(new PropertyGroupDescription("Speaker"));

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

        public bool CanTranslate
            => !_isTranslating
            && SelectedTextLine != null && SelectedTextLine.Language != TranslationLanguage;

        public void Translate()
        {
            // disable Translate button
            IsTranslating = true;
            NotifyOfPropertyChange(() => CanTranslate);

            Task translationJob = new Task(TranslateAction);
            translationJob.Start();
        }

        /// <summary>
        /// Calls the Google Translation Cloud to perform text translation.
        /// </summary>
        public void TranslateAction()
        {
            // we cache these values in case user changed the selected line before the cloud responded
            TextLine selectedLine = SelectedTextLine;
            Language targetLanguage = TranslationLanguage;

            string translation = _healDocumentController.Translate(
                text: selectedLine.Text,
                source: selectedLine.Language.Value,
                target: targetLanguage);

            if (translation == null)
            {
                MessageBox.Show(
                    "Translation error. No Internet connection or Google Translation Cloud is inactive.",
                    "Translation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                selectedLine.TranslatedText = translation;
                selectedLine.TranslationLanguage = targetLanguage;
            }

            // enable button
            IsTranslating = false;
            NotifyOfPropertyChange(() => CanTranslate);

            // in order to update UI form a different thread than the main thread we need to call Dispatcher
            Application.Current.Dispatcher.Invoke(() => _textLinesView.ForceCommitRefresh());
        }

        public bool CanUndo => _commandController.UndoCount > 0;

        public void Undo()
        {
            _commandController.Undo();
            RefreshAllViews();
        }

        public bool CanRedo => _commandController.RedoCount > 0;

        public void Redo()
        {
            _commandController.Redo();
            RefreshAllViews();
        }

        void RefreshAllViews()
        {
            _entriesView?.ForceCommitRefresh();
            _variantsView?.ForceCommitRefresh();
            _textLinesView?.ForceCommitRefresh();
        }

        public bool CanCheckDataConsistency()
        {
            return true;
        }

        public void CheckDataConsistency() => CheckDataConsistencyAsync();

        /// <summary>
        /// Performs the following checks:
        /// - GUID duplication
        /// - mismatching languages (language field is set different than the one recognized by the translation cloud).
        /// - language is empty
        /// - variant's name is empty (impossible to auto fix)
        /// - speaker is empty (impossible to auto fix)
        /// </summary>
        async void CheckDataConsistencyAsync()
        {

        }

        public bool CanHealDocument()
        {
            return true;
        }

        public void HealDocument()
        {
            _healDocumentController.HealDocument(_entries);

            RefreshAllViews();
        }

        public bool CanAddEntry => Entries != null;

        public void AddEntry()
        {
            var entry = new LocalizationEntry();
            Entries.Add(entry);
            _commandController.Push(new AddCommand<LocalizationEntry>(Entries, Entries.Count - 1, entry));

            _entriesView.ForceCommitRefresh();
        }

        public bool CanAddVariant => SelectedEntry != null;

        public void AddVariant()
        {
            SelectedEntry.Variants.Add(new Variant());
            _variantsView.ForceCommitRefresh();
        }

        public bool CanAddTextLine => SelectedVariant != null;

        public void AddTextLine()
        {
            SelectedVariant.TextLines.Add(new TextLine());
            _textLinesView.ForceCommitRefresh();
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

                _commandController.Push(undoCmd);
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

                _commandController.Push(undoCmd);
            }
        }

        // doesn't work for now something is wrong with the XAML structure
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

                _commandController.Push(undoCmd);
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
