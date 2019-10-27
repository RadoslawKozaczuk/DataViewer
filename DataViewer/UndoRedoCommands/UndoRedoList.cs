﻿using System.Collections.Generic;

namespace DataViewer.UndoRedoCommands
{
    /// <summary>
    /// Extends normal <see cref="IList{T}"/> by adding undo/redo functionality.
    /// Base list functionality is untouched so if you want to add/remove element normally
    /// just use basic methods.
    /// <code></code>
    /// Added methods:
    /// <list type="table">
    /// <item>
    /// <term><see cref="AddWithUndoRedoTracking"/></term>
    /// <description>Adds element to the list and adds <see cref="AddCommand{T}"/> to the corresponding <see cref="CommandStack{T}"/></description>
    /// </item>
    /// <item>
    /// <term><see cref="RemoveWithUndoRedoTracking"/></term>
    /// <description>Removes element from the list and adds <see cref="RemoveCommand{T}"/> to the corresponding <see cref="CommandStack{T}"/></description>
    /// </item>
    /// <item>
    /// <term><see cref="RemoveAtWithUndoRedoTracking"/></term>
    /// <description>Removes element from the list and adds <see cref="RemoveCommand{T}"/> to the corresponding <see cref="CommandStack{T}"/></description>
    /// </item>
    /// <item>
    /// <term><see cref="InsertWithUndoRedoTracking"/></term>
    /// <description>Inserts element to the list and adds <see cref="AddCommand{T}"/> to the corresponding <see cref="CommandStack{T}"/></description>
    /// </item>
    /// </list>
    /// </summary>
    public class UndoRedoList<T> : List<T>
    {
        readonly CommandStack<IUndoRedoCommand> _commandStack;

        public UndoRedoList(CommandStack<IUndoRedoCommand> commandStack) 
            : base()
        {
            _commandStack = commandStack;
        }

        public UndoRedoList(int capacity, CommandStack<IUndoRedoCommand> commandStack) 
            : base(capacity)
        {
            _commandStack = commandStack;
        }

        public UndoRedoList(IEnumerable<T> enumerable,  CommandStack<IUndoRedoCommand> commandStack) 
            : base(enumerable)
        {
            _commandStack = commandStack;
        }
        
        public void AddWithUndoRedoTracking(T item)
        {
            Add(item);
            _commandStack.Push(new AddCommand<T>(this, Count - 1, item));
        }

        public void RemoveWithUndoRedoTracking(T item)
        {
            Remove(item);
            _commandStack.Push(new RemoveCommand<T>(this, Count - 1, item));
        }

        public void RemoveAtWithUndoRedoTracking(int index)
        {
            T item = base[index];
            RemoveAt(index);
            _commandStack.Push(new RemoveCommand<T>(this, index, item));
        }

        public void InsertWithUndoRedoTracking(int index, T item) 
            => _commandStack.Push(new AddCommand<T>(this, index, item));
    }
}
