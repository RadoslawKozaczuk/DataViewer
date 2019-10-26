namespace DataViewer
{
    /// <summary>
    /// Languages supported by our system.
    /// </summary>
    public enum Language { English_US, Japanease, French }

    public enum CommandState
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
