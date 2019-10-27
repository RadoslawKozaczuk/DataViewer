using System;
using System.Collections.Generic;

namespace DataViewer.UndoRedo
{
    sealed class AddCommand<T> : AbstractUndoRedoCommand
    {
        readonly IList<T> _list;
        readonly int _idInSequence;
        readonly T _objRef;

        public AddCommand(IList<T> list, int idInSequence, T objRef)
        {
            _list = list;
            _idInSequence = idInSequence;
            _objRef = objRef;
        }

        public override void Undo()
        {
            if (State == UndoRedoCommandState.Redo)
                throw new Exception(UNDO_CONSECUTIVE_CALL_ERROR);

            _list.Remove(_objRef);

            State = UndoRedoCommandState.Redo;
        }

        public override void Redo()
        {
            if (State == UndoRedoCommandState.Undo)
                throw new Exception(REDO_CONSECUTIVE_CALL_ERROR);

            // insert object back into the collection or add it at the end if the previous relative position is out of bounds
            if (_list.Count > _idInSequence)
                _list.Insert(_idInSequence, _objRef);
            else
                _list.Add(_objRef);

            State = UndoRedoCommandState.Undo;
        }
    }
}
