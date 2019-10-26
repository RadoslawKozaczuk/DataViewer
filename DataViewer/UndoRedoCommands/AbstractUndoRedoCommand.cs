﻿namespace DataViewer.UndoRedoCommands
{
    abstract class AbstractUndoRedoCommand : IUndoRedoCommand
    {
        protected const string UNDO_CONSECUTIVE_CALL_ERROR = "Command already undone cannot be undone again. Consider calling ExecuteRedo method.";
        protected const string REDO_CONSECUTIVE_CALL_ERROR = "Command already redone cannot be redone again.Consider calling ExecuteUndo method.";

        public UndoRedoCommandState State { get; protected set; } = UndoRedoCommandState.Undo;

        public abstract void Redo();
        public abstract void Undo();
    }
}
