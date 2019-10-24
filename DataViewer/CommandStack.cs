using DataViewer.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataViewer
{
    static class CommandStack
    {
        public static Stack<UndoCommand> UndoStack = new Stack<UndoCommand>();
        public static Stack<UndoCommand> RedoStack = new Stack<UndoCommand>();
    }
}
