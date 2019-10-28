using DataViewer.Interfaces;
using System;
using System.Collections.Generic;

namespace DataViewer.UndoRedo.Commands
{
    sealed class RemoveCommand<T> : AbstractUndoRedoCommand
        where T : IModel
    {
        public override IModel TargetObject => _objRef;

        public readonly T _objRef;
        readonly IList<T> _list;
        readonly int _idInSequence;

        public RemoveCommand(IList<T> list, int idInSequence, T objRef)
        {
            _list = list;
            _idInSequence = idInSequence;
            _objRef = objRef;
        }

        public override bool CheckExecutionContext()
        {
            if (State == UndoRedoCommandState.Undo)
            {
                if (_list == null) // list no longer exists (was removed externally without undo/redo tracking)
                    return false;
            }
            else if (State == UndoRedoCommandState.Redo)
            {
                if (!_list.Contains(_objRef)) // object no longer exists (was removed externally without undo/redo tracking)
                    return false;
            }

            return true;
        }

        public override void Undo()
        {
            // assertion
            if (State == UndoRedoCommandState.Redo)
                throw new Exception(UNDO_CONSECUTIVE_CALL_ERROR);

            // insert object back into the collection or add it at the end if the previous relative position is out of bounds
            if (_list.Count > _idInSequence)
                _list.Insert(_idInSequence, _objRef);
            else
                _list.Add(_objRef);

            State = UndoRedoCommandState.Redo;
        }

        public override void Redo()
        {
            // assertion
            if (State == UndoRedoCommandState.Undo)
                throw new Exception(REDO_CONSECUTIVE_CALL_ERROR);

            _list.Remove(_objRef);
            State = UndoRedoCommandState.Undo;
        }
    }
}
