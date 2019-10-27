namespace DataViewer.UndoRedo
{
    public enum UndoRedoCommandState
    {
        /// <summary>
        /// Command is ready to perform Undo action.
        /// </summary>
        Undo,
        /// <summary>
        /// Command is ready to perform Redo action.
        /// </summary>
        Redo
    }
}
