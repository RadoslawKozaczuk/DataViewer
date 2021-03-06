﻿using DataViewer.Interfaces;
using DataViewer.UndoRedo.Commands;
using System;
using System.Collections.Generic;

namespace DataViewer.UndoRedo
{
    public class CommandStack
    {
        public int UndoCount => _pointer;
        public int RedoCount => _undoRedoStack.Count - _pointer;

        readonly IList<IUndoRedoCommand> _undoRedoStack = new List<IUndoRedoCommand>();
        int _pointer; // points at first redo - so first undo will be _pointer - 1

        readonly Action _onUndoAction;
        readonly Action _onRedoAction;

        /// <summary>
        /// Optional parameters allow to pass an action that should be executed each time command stack's state is changed.
        /// </summary>
        public CommandStack(Action onUndoAction = null, Action onRedoAction = null)
        {
            _onUndoAction = onUndoAction;
            _onRedoAction = onRedoAction;
        }

        /// <summary>
        /// Automatically pushes given <see cref="IUndoRedoCommand"/> command on the Undo stack.
        /// </summary>
        public void Push(IUndoRedoCommand cmd)
        {
            // assertion
#if DEBUG
            if (cmd == null)
                throw new ArgumentNullException("cmd", "cmd parameter cannot be null.");
#endif

            if (cmd is EditCommand<IModel>)
            {
                // check if rely on anything else (only Edit and Remove)
                for (int i = 0; i < _undoRedoStack.Count; i++)
                {
                    IUndoRedoCommand c = _undoRedoStack[i];
                    if (cmd.GetType().GetGenericTypeDefinition() == typeof(AddCommand<>)
                        || cmd.GetType().GetGenericTypeDefinition() == typeof(RemoveCommand<>))
                    {
                        if (cmd.TargetObject == c.TargetObject)
                            (cmd as EditCommand<IModel>).RelyOn = c;
                    }

                    // this would not work
                    //if (cmd is AddCommand<IModel>)
                }
            }

            _undoRedoStack.Insert(_pointer++, cmd);
            _onUndoAction?.Invoke();
            _onRedoAction?.Invoke();
        }

        public void Undo()
        {
            IUndoRedoCommand cmd = _undoRedoStack[--_pointer];
            if (cmd.CheckExecutionContext()) // if command is no longer valid dispose it
                cmd.Undo();
            else
                _undoRedoStack.RemoveAt(_pointer);

            _onUndoAction?.Invoke();
            _onRedoAction?.Invoke();
        }

        public void Redo()
        {
            IUndoRedoCommand cmd = _undoRedoStack[_pointer];
            if (cmd.CheckExecutionContext()) // if command is no longer valid dispose it
            {
                cmd.Redo();
                _pointer++;
            }
            else
                _undoRedoStack.RemoveAt(_pointer);

            _onUndoAction?.Invoke();
            _onRedoAction?.Invoke();
        }

        /// <summary>
        /// Iterate over all commands and delete those no longer valid.
        /// Call it whenever data set was modified externally without undo/redo tracking.
        /// </summary>
        public void Refresh()
        {
            // edit command relies on add and remove so we have to first iterate over these
            for (int i = 0; i < _undoRedoStack.Count; i++)
            {
                IUndoRedoCommand cmd = _undoRedoStack[i];
                if (cmd is AddCommand<IModel> || cmd is RemoveCommand<IModel>)
                    if (!cmd.CheckExecutionContext())
                    {
                        _undoRedoStack.RemoveAt(i--);
                        _pointer--;
                    }
            }

            for (int i = 0; i < _undoRedoStack.Count; i++)
            {
                IUndoRedoCommand cmd = _undoRedoStack[i];
                if (cmd is EditCommand<IModel>)
                    if (!cmd.CheckExecutionContext())
                    {
                        _undoRedoStack.RemoveAt(i--);
                        _pointer--;
                    }
            }
        }

        public void Clear()
        {
            _undoRedoStack.Clear();
            _pointer = 0;
        }
    }
}
