﻿using DataViewer.Input;
using DataViewer.Interfaces;
using DataViewer.Models;
using DataViewer.UndoRedo;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace DataViewer.ViewModels
{
    // this partial class contains all non-public members of the ShellViewModel
    // the division to increase the code readability
    public partial class ShellViewModel : ViewModelBase, IShellViewModel
    {
        // controllers
        readonly IDataIntegrityController _dataIntegrityController;
        readonly ITranslationCloudAdapter _translationCloud;
        readonly CommandStack<IUndoRedoCommand> _commandStack;

        // views
        ListCollectionView _entriesView;
        ListCollectionView _variantsView;
        ListCollectionView _textLinesView;

        // models
        UndoRedoList<LocalizationEntry> _entries;
        LocalizationEntry _selectedEntry;
        Variant _selectedVariant;
        TextLine _selectedTextLine;
        Language _translationLanguage;
        string _speakerFilter;
        string _guidFilter;
        string _nameFilter;
        string _textFilter;
        bool _isTranslating;
        bool _isDataConsistent = true;

        // this is necessary to circumnavigate custom data template limitations
        (TextLine textLine, TextLine oldTextLineCopy, string header) _tempValues;

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

        /// <summary>
        /// Calls the Google Translation Cloud to perform text translation.
        /// </summary>
        void TranslateAction()
        {
            // we cache these values in case user changed the selected line before the cloud responded
            TextLine selectedLine = SelectedTextLine;
            Language targetLanguage = TranslationLanguage;

            bool success = _translationCloud.Translate(
                text: selectedLine.Text,
                source: selectedLine.Language.Value,
                target: targetLanguage,
                out string translation);

            if (success)
            {
                selectedLine.TranslatedText = translation;
                selectedLine.TranslationLanguage = targetLanguage;
            }
            else
            {
                MessageBox.Show(
                    "Translation error. No Internet connection or Google Translation Cloud is inactive.",
                    "Translation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            // enable button
            IsTranslating = false;
            NotifyOfPropertyChange(() => CanTranslate);

            // in order to update UI form a different thread than the main thread we need to call Dispatcher
            Application.Current.Dispatcher.Invoke(() => _textLinesView.ForceCommitRefresh());
        }

        void RefreshAllViews()
        {
            _entriesView?.ForceCommitRefresh();
            _variantsView?.ForceCommitRefresh();
            _textLinesView?.ForceCommitRefresh();
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
