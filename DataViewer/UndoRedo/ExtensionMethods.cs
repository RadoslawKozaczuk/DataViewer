using DataViewer.Interfaces;
using System.Collections.Generic;

namespace DataViewer.UndoRedo
{
    static class ExtensionMethods
    {
        public static UndoRedoList<T> ConvertToUndoRedoList<T>(this IEnumerable<T> list, CommandStack commandStack)
            where T : IModel
            => new UndoRedoList<T>(list, commandStack);
    }
}
