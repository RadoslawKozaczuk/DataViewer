using DataViewer.Interfaces;

namespace DataViewer.UndoRedo
{
    public interface IUndoRedoCommand
    {
        IModel TargetObject { get; }
        UndoRedoCommandState State { get; }
        /// <summary>
        /// Checks if command is still valid (command may not be valid if the referenced object 
        /// was deleted externally without undo/redo tracking).
        /// Call it before calling <see cref="Undo"/> or <see cref="Redo"/> methods to see if the command is still valid.
        /// </summary>
        bool CheckExecutionContext();
        /// <summary>
        /// Undo the last command.
        /// </summary>
        void Undo();
        /// <summary>
        /// Redo the last command.
        /// </summary>
        void Redo();
    }
}
