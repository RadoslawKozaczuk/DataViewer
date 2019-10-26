namespace DataViewer.UndoRedoCommands
{
    interface IUndoRedoCommand
    {
        CommandState State { get; }
        void ExecuteUndo();
        void ExecuteRedo();
    }
}
