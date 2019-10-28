using DataViewer.Interfaces;
using DataViewer.Models;
using DataViewer.UndoRedo;
using DataViewer.UndoRedo.Commands;
using Microsoft.Win32;
using MoreLinq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DataViewer.ViewModels
{
    public partial class ShellViewModel : ViewModelBase, IShellViewModel
    {
        #region Properties
        public UndoRedoList<LocalizationEntry> Entries
        {
            get => _entries;
            set
            {
                Set(ref _entries, value);
                NotifyOfPropertyChange(() => CanAddEntry);
            }
        }

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

                    NotifyOfPropertyChange(() => CanDeleteEntry);
                }
            }
        }

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

                    NotifyOfPropertyChange(() => CanDeleteVariant);
                }
            }
        }

        public TextLine SelectedTextLine
        {
            get => _selectedTextLine;
            set
            {
                Set(ref _selectedTextLine, value); // this one-liner does both set and notify
                NotifyOfPropertyChange(() => CanTranslate);
                NotifyOfPropertyChange(() => CanDeleteTextLine);
            }
        }

        public Language TranslationLanguage
        {
            get => _translationLanguage;
            set
            {
                _translationLanguage = value;
                NotifyOfPropertyChange(() => CanTranslate);
            }
        }

        public string SpeakerFilter
        {
            get => _speakerFilter;
            set
            {
                _speakerFilter = value;
                _entriesView?.ForceCommitRefresh();
            }
        }

        public string GUIDFilter
        {
            get => _guidFilter;
            set
            {
                _guidFilter = value;
                _entriesView?.ForceCommitRefresh();
            }
        }

        public string NameFilter
        {
            get => _nameFilter;
            set
            {
                _nameFilter = value;
                _variantsView?.ForceCommitRefresh();
            }
        }

        public string TextFilter
        {
            get => _textFilter;
            set
            {
                _textFilter = value;
                _textLinesView?.ForceCommitRefresh();
            }
        }

        public bool IsProcessingBackgroundTask
        {
            get => _isProcessingBackgroundTask;
            set
            {
                Set(ref _isProcessingBackgroundTask, value);
                NotifyOfPropertyChange(() => CanTranslate);
                NotifyOfPropertyChange(() => CanScan);
                NotifyOfPropertyChange(() => CanHeal);
            }
        }

        public string StatusBarInfo
        {
            get => _statusBarInfo;
            private set => Set(ref _statusBarInfo, value);
        }

        #endregion

        #region Guard Methods (properties)
        public bool CanExportToExcel => Entries != null && Entries.Count != 0;

        public bool CanTranslate
            => !IsProcessingBackgroundTask
            && SelectedTextLine != null 
            && SelectedTextLine.Language != TranslationLanguage
            && !string.IsNullOrWhiteSpace(SelectedTextLine.Text);

        public bool CanUndo => _commandStack.UndoCount > 0;

        public bool CanRedo => _commandStack.RedoCount > 0;

        public bool CanScan => Entries != null && Entries.Count > 0;

        public bool CanHeal => !_isDataConsistent;

        public bool CanAddEntry => Entries != null;

        public bool CanAddVariant => SelectedEntry != null;

        public bool CanAddTextLine => SelectedVariant != null;

        public bool CanDeleteEntry => SelectedEntry != null;

        public bool CanDeleteVariant => SelectedVariant != null;

        public bool CanDeleteTextLine => SelectedTextLine != null;
        #endregion

        public ShellViewModel(IDataIntegrityController dataIntegrityController, ITranslationCloudAdapter translationCloudAdapter)
            : base()
        {
            _dataIntegrityController = dataIntegrityController;
            _translationCloud = translationCloudAdapter;

            // for convenience we pass notifiers to command stack so whenever an operation is executed on it, notifiers will also be called
            _commandStack = new CommandStack<IUndoRedoCommand>(
                notifyUndoAction: () =>
                {
                    NotifyOfPropertyChange(() => CanUndo);
                    RefreshAllViews();
                },
                notifyRedoAction: () =>
                {
                    NotifyOfPropertyChange(() => CanRedo);
                    RefreshAllViews();
                });
        }

        #region Public Methods
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

            // we don't want models or serializers to know anything undo/redo specific therefore 
            // the list conversion needs to be done here
            Entries = entries.ConvertToUndoRedoList(_commandStack);
            foreach (LocalizationEntry entry in Entries)
            {
                entry.Variants = entry.Variants.ConvertToUndoRedoList(_commandStack);
                entry.Variants.ForEach(v => v.TextLines = v.TextLines.ConvertToUndoRedoList(_commandStack));
            }

            _entriesView = (ListCollectionView)CollectionViewSource.GetDefaultView(Entries);
            _entriesView.Filter = EntryFilter;

            // Grouping disables virtualization. This can bring huge performance issues on large data sets.
            _entriesView.GroupDescriptions.Add(new PropertyGroupDescription("Speaker"));

            NotifyOfPropertyChange(() => CanExportToExcel);
            NotifyOfPropertyChange(() => CanScan);
        }

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

        public void Translate() => Task.Run(() => TranslateAction());

        public void Undo() => _commandStack.Undo();

        public void Redo() => _commandStack.Redo();

        public void Scan() => Task.Run(() => ScanAction());

        public void Heal() => Task.Run(() => HealAction());

        public void AddEntry()
        {
            var entry = new LocalizationEntry();
            entry.Variants = entry.Variants.ConvertToUndoRedoList(_commandStack);
            Entries.AddWithUndoRedoTracking(entry);
        }

        public void AddVariant()
        {
            var variant = new Variant();
            variant.TextLines = variant.TextLines.ConvertToUndoRedoList(_commandStack);
            (SelectedEntry.Variants as UndoRedoList<Variant>).AddWithUndoRedoTracking(variant);
        }

        public void AddTextLine() => (SelectedVariant.TextLines as UndoRedoList<TextLine>).AddWithUndoRedoTracking(new TextLine());

        public void DeleteEntry() => Entries.RemoveWithUndoRedoTracking(SelectedEntry);

        public void DeleteVariant() => (SelectedEntry.Variants as UndoRedoList<Variant>).RemoveWithUndoRedoTracking(SelectedVariant);

        public void DeleteTextLine() => (SelectedVariant.TextLines as UndoRedoList<TextLine>).RemoveWithUndoRedoTracking(SelectedTextLine);
        #endregion

        #region Event Handlers
        public void Entries_CellEditEnding(DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
                return;

            if (e.Column.Header.ToString() == "Speaker")
            {
                string newSpeaker = ((TextBox)e.EditingElement).Text;

                var undoCmd = new EditCommand(
                    objRef: SelectedEntry,
                    oldValue: new LocalizationEntry { Speaker = SelectedEntry.Speaker },
                    newValue: new LocalizationEntry { Speaker = newSpeaker });

                SelectedEntry.Speaker = newSpeaker;
                _commandStack.Push(undoCmd);
            }

            _entriesView.ForceCommitRefresh();
        }

        public void Variants_CellEditEnding(DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
                return;

            if (e.Column.Header.ToString() == "Name")
            {
                string newName = ((TextBox)e.EditingElement).Text;
                var undoCmd = new EditCommand(
                    objRef: SelectedVariant,
                    oldValue: new Variant { Name = SelectedVariant.Name },
                    newValue: new Variant { Name = newName });

                SelectedVariant.Name = newName;
                _commandStack.Push(undoCmd);
            }

            _variantsView.ForceCommitRefresh();
        }
        
        public void TextLines_CellEditEnding(DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
                return;

            var entry = Entries[Entries.IndexOf(SelectedEntry)];
            var variant = entry.Variants[entry.Variants.IndexOf(SelectedVariant)];
            _tempValues.textLine = variant.TextLines[variant.TextLines.IndexOf(SelectedTextLine)];
            _tempValues.oldTextLineCopy = new TextLine(_tempValues.textLine);
            _tempValues.header = e.Column.Header.ToString();
        }

        public void TextLines_SelectedCellsChanged()
        {
            if (_tempValues.header == null)
                return;

            if (_tempValues.header == "Text")
            {
                var undoCmd = new EditCommand(
                    objRef: _tempValues.textLine,
                    oldValue: new TextLine { Text = _tempValues.oldTextLineCopy.Text },
                    newValue: new TextLine { Text = _tempValues.textLine.Text });

                _tempValues.textLine.Text = _tempValues.textLine.Text;

                _commandStack.Push(undoCmd);
            }
            else if (_tempValues.header == "Language")
            {
                var undoCmd = new EditCommand(
                    objRef: _tempValues.textLine,
                    oldValue: new TextLine { Language = _tempValues.oldTextLineCopy.Language },
                    newValue: new TextLine { Language = _tempValues.textLine.Language });

                _tempValues.textLine.Language = _tempValues.textLine.Language;

                _commandStack.Push(undoCmd);
            }

            NotifyOfPropertyChange(() => SelectedTextLine);

            _tempValues.textLine = null;
            _tempValues.oldTextLineCopy = null;
            _tempValues.header = null;

            _textLinesView.ForceCommitRefresh();
        }
        #endregion
    }
}
