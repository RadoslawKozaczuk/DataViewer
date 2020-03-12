using DataViewer.Interfaces;
using DataViewer.UndoRedo.Commands;
using System.Collections.Generic;

namespace DataViewer.UndoRedo
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
    /// <description>Adds element to the list and adds <see cref="AddCommand{T}"/> to the corresponding <see cref="CommandStack"/></description>
    /// </item>
    /// <item>
    /// <term><see cref="RemoveWithUndoRedoTracking"/></term>
    /// <description>Removes element from the list and adds <see cref="RemoveCommand{T}"/> to the corresponding <see cref="CommandStack"/></description>
    /// </item>
    /// <item>
    /// <term><see cref="RemoveAtWithUndoRedoTracking"/></term>
    /// <description>Removes element from the list and adds <see cref="RemoveCommand{T}"/> to the corresponding <see cref="CommandStack"/></description>
    /// </item>
    /// <item>
    /// <term><see cref="InsertWithUndoRedoTracking"/></term>
    /// <description>Inserts element to the list and adds <see cref="AddCommand{T}"/> to the corresponding <see cref="CommandStack"/></description>
    /// </item>
    /// </list>
    /// </summary>
    public class UndoRedoList<T> : List<T>
        where T : IModel
    {
        readonly CommandStack _commandStack;

        public UndoRedoList(CommandStack commandStack) 
            : base()
        {
            // assertion
#if DEBUG
            if (commandStack == null)
                throw new System.ArgumentNullException("commandStack", "commandStack parameter cannot be null");
#endif

            _commandStack = commandStack;
        }

        public UndoRedoList(int capacity, CommandStack commandStack) 
            : base(capacity)
        {
            // assertion
#if DEBUG
            if (commandStack == null)
                throw new System.ArgumentNullException("commandStack", "commandStack parameter cannot be null");
#endif

            _commandStack = commandStack;
        }

        public UndoRedoList(IEnumerable<T> enumerable,  CommandStack commandStack) 
            : base(enumerable)
        {
            // assertion
#if DEBUG
            if (commandStack == null)
                throw new System.ArgumentNullException("commandStack", "commandStack parameter cannot be null");
#endif

            _commandStack = commandStack;
        }

        /// <summary>
        /// Adds element to the list and adds <see cref="AddCommand{T}"/> to the corresponding <see cref="CommandStack"/>
        /// </summary>
        public void AddWithUndoRedoTracking(T item)
        {
            Add(item);
            _commandStack.Push(new AddCommand<T>(this, Count - 1, item));
        }

        /// <summary>
        /// Removes element from the list and adds<see cref="RemoveCommand{T}"/> to the corresponding<see cref= "CommandStack" />
        /// </summary>
        public void RemoveWithUndoRedoTracking(T item)
        {
            Remove(item);
            _commandStack.Push(new RemoveCommand<T>(this, Count - 1, item));
        }

        /// <summary>
        /// Removes element from the list and adds <see cref="RemoveCommand{T}"/> to the corresponding <see cref="CommandStack"/>
        /// </summary>
        public void RemoveAtWithUndoRedoTracking(int index)
        {
            T item = base[index];
            RemoveAt(index);
            _commandStack.Push(new RemoveCommand<T>(this, index, item));
        }

        /// <summary>
        /// Inserts element to the list and adds <see cref="AddCommand{T}"/> to the corresponding <see cref="CommandStack"/>
        /// </summary>
        public void InsertWithUndoRedoTracking(int index, T item) 
            => _commandStack.Push(new AddCommand<T>(this, index, item));
    }
}
