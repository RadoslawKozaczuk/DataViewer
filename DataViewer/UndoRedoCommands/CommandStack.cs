using System;
using System.Collections.Generic;

namespace DataViewer.UndoRedoCommands
{
    public class CommandStack<T> where T : IUndoRedoCommand
    {
        public int UndoCount => _undoStack.Count;
        public int RedoCount => _redoStack.Count;

        readonly Stack<T> _undoStack = new Stack<T>();
        readonly Stack<T> _redoStack = new Stack<T>();
        readonly Action _notifyUndoAction;
        readonly Action _notifyRedoAction;

        /// <summary>
        /// Optional parameters allow to pass an action that should be executed each time command stack's state is changed.
        /// </summary>
        public CommandStack(Action notifyUndoAction = null, Action notifyRedoAction = null)
        {
            _notifyUndoAction = notifyUndoAction;
            _notifyRedoAction = notifyRedoAction;
        }

        /// <summary>
        /// Automatically pushes given <see cref="IUndoRedoCommand"/> command on the Undo stack.
        /// Whenever a new commands is pushed all Redo stack commands are cleared.
        /// </summary>
        public void Push(T cmd)
        {
            _undoStack.Push(cmd);
            _redoStack.Clear();
            _notifyUndoAction?.Invoke();
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
