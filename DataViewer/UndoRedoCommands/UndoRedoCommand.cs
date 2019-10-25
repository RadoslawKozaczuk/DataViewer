using DataViewer.Models;
using System;

namespace DataViewer.UndoRedoCommands
{
    /// <summary>
    /// A command is Undo or Redo based on context.
    /// </summary>
    sealed class UndoRedoCommand : IUndoRedoCommand
    {
        // commands are impossible to be created in redo state, the only way to achieve redo state is to perform undo action while in Undo state
        public CommandState State { get; private set; } = CommandState.Undo;

        // if something is null then it was not modified by the command
        public LocalizationEntry LocalizationEntryRef;
        public Variant VariantRef;
        public TextLine TextLineRef;

        // values before change
        public LocalizationEntry OldEntry;
        public Variant OldVariant;
        public TextLine OldTextLine;

        // values after change
        public LocalizationEntry NewEntry;
        public Variant NewVariant;
        public TextLine NewTextLine;

        public UndoRedoCommand(LocalizationEntry objRef, LocalizationEntry oldValue, LocalizationEntry newValue)
        {
            LocalizationEntryRef = objRef;
            OldEntry = oldValue;
            NewEntry = newValue;
        }

        public UndoRedoCommand(Variant objRef, Variant oldValue, Variant newValue)
        {
            VariantRef = objRef;
            OldVariant = oldValue;
            NewVariant = newValue;
        }

        public UndoRedoCommand(TextLine objRef, TextLine oldValue, TextLine newValue)
        {
            TextLineRef = objRef;
            OldTextLine = oldValue;
            NewTextLine = newValue;
        }

        /// <summary>
        /// Reverse the action performed by this action.
        /// </summary>
        public void ExecuteUndo()
        {
            if (State == CommandState.Redo)
                throw new Exception("Command already undone cannot be undone again. Consider calling ExecuteRedo method.");

            if (OldEntry != null)
            {
                if (OldEntry.Speaker != null)
                    LocalizationEntryRef.Speaker = OldEntry.Speaker;
            }

            if (OldVariant != null)
            {
                if (OldVariant.Name != null)
                    VariantRef.Name = OldVariant.Name;
            }

            if (OldTextLine != null)
            {
                if (OldTextLine.Language != null)
                    TextLineRef.Language = OldTextLine.Language;

                if (OldTextLine.Text != null)
                    TextLineRef.Text = OldTextLine.Text;
            }

            State = CommandState.Redo;
        }

        /// <summary>
        /// Applies the changes performed by this action (only possible if the action was previously undone).
        /// </summary>
        public void ExecuteRedo()
        {
            if (State == CommandState.Undo)
                throw new Exception("Command already redone cannot be redone again. Consider calling ExecuteUndo method.");

            if (NewEntry != null)
            {
                if (NewEntry.Speaker != null)
                    LocalizationEntryRef.Speaker = NewEntry.Speaker;
            }

            if (NewVariant != null)
            {
                if (NewVariant.Name != null)
                    VariantRef.Name = NewVariant.Name;
            }

            if (NewTextLine != null)
            {
                if (NewTextLine.Language != null)
                    TextLineRef.Language = NewTextLine.Language;

                if (NewTextLine.Text != null)
                    TextLineRef.Text = NewTextLine.Text;
            }

            State = CommandState.Undo;
        }
    }
}
