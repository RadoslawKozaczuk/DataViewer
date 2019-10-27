using DataViewer.Models;
using System;

namespace DataViewer.UndoRedo.Commands
{
    /// <summary>
    /// A command is Undo or Redo based on context.
    /// </summary>
    sealed class EditCommand : AbstractUndoRedoCommand
    {
        // if something is null then it was not modified by the command
        readonly LocalizationEntry _localizationEntryRef;
        readonly Variant _variantRef;
        readonly TextLine _textLineRef;

        // values before change
        readonly LocalizationEntry _oldEntry;
        readonly Variant _oldVariant;
        readonly TextLine _oldTextLine;

        // values after change
        readonly LocalizationEntry _newEntry;
        readonly Variant _newVariant;
        readonly TextLine _newTextLine;

        public EditCommand(LocalizationEntry objRef, LocalizationEntry oldValue, LocalizationEntry newValue)
        {
            _localizationEntryRef = objRef;
            _oldEntry = oldValue;
            _newEntry = newValue;
        }

        public EditCommand(Variant objRef, Variant oldValue, Variant newValue)
        {
            _variantRef = objRef;
            _oldVariant = oldValue;
            _newVariant = newValue;
        }

        public EditCommand(TextLine objRef, TextLine oldValue, TextLine newValue)
        {
            _textLineRef = objRef;
            _oldTextLine = oldValue;
            _newTextLine = newValue;
        }

        /// <summary>
        /// Reverse the action performed by this action.
        /// </summary>
        public override void Undo()
        {
            if (State == UndoRedoCommandState.Redo)
                throw new Exception(UNDO_CONSECUTIVE_CALL_ERROR);

            if (_oldEntry != null)
            {
                if (_oldEntry.Speaker != null)
                    _localizationEntryRef.Speaker = _oldEntry.Speaker;
            }

            if (_oldVariant != null)
            {
                if (_oldVariant.Name != null)
                    _variantRef.Name = _oldVariant.Name;
            }

            if (_oldTextLine != null)
            {
                if (_oldTextLine.Language != null)
                    _textLineRef.Language = _oldTextLine.Language;

                if (_oldTextLine.Text != null)
                    _textLineRef.Text = _oldTextLine.Text;
            }

            State = UndoRedoCommandState.Redo;
        }

        /// <summary>
        /// Applies the changes performed by this action (only possible if the action was previously undone).
        /// </summary>
        public override void Redo()
        {
            if (State == UndoRedoCommandState.Undo)
                throw new Exception(REDO_CONSECUTIVE_CALL_ERROR);

            if (_newEntry != null)
            {
                if (_newEntry.Speaker != null)
                    _localizationEntryRef.Speaker = _newEntry.Speaker;
            }

            if (_newVariant != null)
            {
                if (_newVariant.Name != null)
                    _variantRef.Name = _newVariant.Name;
            }

            if (_newTextLine != null)
            {
                if (_newTextLine.Language != null)
                    _textLineRef.Language = _newTextLine.Language;

                if (_newTextLine.Text != null)
                    _textLineRef.Text = _newTextLine.Text;
            }

            State = UndoRedoCommandState.Undo;
        }
    }
}
