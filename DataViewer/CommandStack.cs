using DataViewer.Commands;
using System.Collections.Generic;

namespace DataViewer
{
    static class CommandStack
    {
        public static Stack<UndoCommand> UndoStack = new Stack<UndoCommand>();
        public static Stack<UndoCommand> RedoStack = new Stack<UndoCommand>();
    }
}
