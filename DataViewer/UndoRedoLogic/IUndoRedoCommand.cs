namespace DataViewer.UndoRedo
{
    public interface IUndoRedoCommand
    {
        UndoRedoCommandState State { get; }
        void Undo();
        void Redo();
    }
}
