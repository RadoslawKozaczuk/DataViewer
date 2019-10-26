namespace DataViewer.UndoRedoCommands
{
    interface IUndoRedoCommand
    {
        UndoRedoCommandState State { get; }
        void Undo();
        void Redo();
    }
}
