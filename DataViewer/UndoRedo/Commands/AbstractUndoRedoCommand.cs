using DataViewer.Interfaces;

namespace DataViewer.UndoRedo.Commands
{
    abstract class AbstractUndoRedoCommand : IUndoRedoCommand
    {
        protected const string UNDO_CONSECUTIVE_CALL_ERROR = "Command already undone cannot be undone again. Consider calling ExecuteRedo method.";
        protected const string REDO_CONSECUTIVE_CALL_ERROR = "Command already redone cannot be redone again. Consider calling ExecuteUndo method.";

        public UndoRedoCommandState State { get; protected set; } = UndoRedoCommandState.Undo;

        public abstract IModel TargetObject { get; }

        public abstract bool CheckExecutionContext();
        public abstract void Undo();
        public abstract void Redo();
    }
}
