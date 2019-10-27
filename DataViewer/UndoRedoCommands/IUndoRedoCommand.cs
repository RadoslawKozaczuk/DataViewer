namespace DataViewer.UndoRedoCommands
{
    public interface IUndoRedoCommand
    {
        UndoRedoCommandState State { get; }
        void Undo();
        void Redo();
    }
}
