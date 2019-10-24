using DataViewer.Commands;
using System;
using System.Collections.Generic;

namespace DataViewer
{
    static class CommandStack
    {
        static readonly Stack<UndoRedoCommand> _undoStack = new Stack<UndoRedoCommand>();
        static readonly Stack<UndoRedoCommand> _redoStack = new Stack<UndoRedoCommand>();

        public static Action NotifyUndoAction;
        public static Action NotifyRedoAction;

        public static int UndoCount => _undoStack.Count;
        public static int RedoCount => _redoStack.Count;

        /// <summary>
        /// Automatically pushes given command on the appropriate stack based on the State variable.
        /// Pushing on undo stack results in redo stack being cleared out.
        /// </summary>
        public static void Push(UndoRedoCommand cmd)
        {
            if (cmd.State == CommandState.ExecutedUndo)
            {
                _undoStack.Push(cmd);
                _redoStack.Clear();
                NotifyUndoAction?.Invoke();
            }
            else
                _redoStack.Push(cmd);

            NotifyRedoAction?.Invoke();
        }

        public static void Undo()
        {
            UndoRedoCommand cmd = _undoStack.Pop();
            cmd.ExecuteUndo();
            _redoStack.Push(cmd);
            
            NotifyUndoAction?.Invoke();
            NotifyRedoAction?.Invoke();
        }

        public static void Redo()
        {
            UndoRedoCommand cmd = _redoStack.Pop();
            cmd.ExecuteRedo();
            _undoStack.Push(cmd);

            NotifyUndoAction?.Invoke();
            NotifyRedoAction?.Invoke();
        }
    }
}
