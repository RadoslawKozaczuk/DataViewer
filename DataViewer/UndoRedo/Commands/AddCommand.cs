using DataViewer.Interfaces;
using System;
using System.Collections.Generic;

namespace DataViewer.UndoRedo.Commands
{
    sealed class AddCommand<T> : AbstractUndoRedoCommand
        where T : IModel
    {
        public override IModel TargetObject => _objRef;

        readonly T _objRef;
        readonly IList<T> _list;
        readonly int _idInSequence;

        public AddCommand(IList<T> list, int idInSequence, T objRef)
        {
            _list = list;
            _idInSequence = idInSequence;
            _objRef = objRef;
        }

        public override bool CheckExecutionContext()
        {
            if(State == UndoRedoCommandState.Undo)
            {
                if (!_list.Contains(_objRef)) // object no longer exists (was removed externally without undo/redo tracking)
                    return false;
            }
            else if(State == UndoRedoCommandState.Redo)
            {
                if (_list == null) // list no longer exists (was removed externally without undo/redo tracking)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if all went OK, false if the referenced object is no longer available 
        /// (was removed without undo/redo tracking).
        /// </summary>
        public override void Undo()
        {
#if DEBUG
            // assertion
            if (State == UndoRedoCommandState.Redo)
                throw new Exception(UNDO_CONSECUTIVE_CALL_ERROR);
#endif

            _list.Remove(_objRef);
            State = UndoRedoCommandState.Redo;
        }

        public override void Redo()
        {
#if DEBUG
            // assertion
            if (State == UndoRedoCommandState.Undo)
                throw new Exception(REDO_CONSECUTIVE_CALL_ERROR);
#endif

            if (_list.Count > _idInSequence)
                _list.Insert(_idInSequence, _objRef);
            else
                _list.Add(_objRef);

            State = UndoRedoCommandState.Undo;
        }
    }
}
