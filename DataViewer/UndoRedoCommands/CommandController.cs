using System;
using System.Collections.Generic;

namespace DataViewer.UndoRedoCommands
{
    class CommandController<T> where T : IUndoRedoCommand
    {
        public int UndoCount => _undoStack.Count;
        public int RedoCount => _redoStack.Count;

        readonly Stack<T> _undoStack = new Stack<T>();
        readonly Stack<T> _redoStack = new Stack<T>();
        readonly Action _notifyUndoAction;
        readonly Action _notifyRedoAction;

        public CommandController(Action notifyUndoAction = null, Action notifyRedoAction = null)
        {
            _notifyUndoAction = notifyUndoAction;
            _notifyRedoAction = notifyRedoAction;
        }

        /// <summary>
        /// Automatically pushes given command on the appropriate stack based on the State variable.
        /// Pushing on undo stack results in redo stack being cleared out.
        /// </summary>
        public void Push(T cmd)
        {
            if (cmd.State == UndoRedoCommandState.Undo)
            {
                _undoStack.Push(cmd);
                _redoStack.Clear();
                _notifyUndoAction?.Invoke();
            }
            else
                _redoStack.Push(cmd);

            _notifyRedoAction?.Invoke();
        }

        public void Undo()
        {
            T cmd = _undoStack.Pop();
            cmd.Undo();
            _redoStack.Push(cmd);

            _notifyUndoAction?.Invoke();
            _notifyRedoAction?.Invoke();
        }

        public void Redo()
        {
            T cmd = _redoStack.Pop();
            cmd.Redo();
            _undoStack.Push(cmd);

            _notifyUndoAction?.Invoke();
            _notifyRedoAction?.Invoke();
        }
    }
}
