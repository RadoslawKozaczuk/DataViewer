using DataViewer.Interfaces;
using DataViewer.Models;
using System;

namespace DataViewer.UndoRedo.Commands
{
    sealed class EditCommand<T> : AbstractUndoRedoCommand
        where T : IModel
    {
        public override IModel TargetObject => _objRef;
        public IUndoRedoCommand RelyOn; // set by CommandStack

        readonly IModel _objRef;
        readonly IModel _oldVal;
        readonly IModel _newVal;
        readonly Type _type;

        public EditCommand(IModel oldValue, IModel newValue, IModel objRef)
        {
            _oldVal = oldValue;
            _newVal = newValue;
            _objRef = objRef;
            _type = typeof(T);
        }

        public override bool CheckExecutionContext()
        {
            // if I have any Add/Remove command that I rely on
            // check if it exists
            if(RelyOn != null)
                return true;

            // if I target data that no Edit/Remove command targets
            // check if that data exists
            if (_objRef != null)
                return true;

            // otherwise false
            return false;
        }

        public override void Undo()
        {
            if (State == UndoRedoCommandState.Redo)
                throw new Exception(UNDO_CONSECUTIVE_CALL_ERROR);

            if (_type == typeof(LocalizationEntry))
                UndoForLocalizationEntry();
            else if (_type == typeof(Variant))
                UndoForVariant();
            else
                UndoForTextLine();

            State = UndoRedoCommandState.Redo;
        }

        public override void Redo()
        {
            if (State == UndoRedoCommandState.Undo)
                throw new Exception(REDO_CONSECUTIVE_CALL_ERROR);

            if(_type == typeof(LocalizationEntry))
                RedoForLocalizationEntry();
            else if (_type == typeof(Variant))
                RedoForVariant();
            else
                RedoForTextLine();

            State = UndoRedoCommandState.Undo;
        }

        void UndoForLocalizationEntry() 
            => (_objRef as LocalizationEntry).Speaker = (_oldVal as LocalizationEntry).Speaker;

        void RedoForLocalizationEntry() 
            => (_objRef as LocalizationEntry).Speaker = (_newVal as LocalizationEntry).Speaker;

        void UndoForVariant()
            => (_objRef as Variant).Name = (_oldVal as Variant).Name;

        void RedoForVariant()
            => (_objRef as Variant).Name = (_newVal as Variant).Name;

        void UndoForTextLine()
        {
            TextLine obj = _objRef as TextLine;
            TextLine old = _oldVal as TextLine;
            TextLine @new = _newVal as TextLine;
            if (old.Text != @new.Text)
                obj.Text = old.Text;
            if (old.Language != @new.Language)
                obj.Language = old.Language;
        }

        void RedoForTextLine()
        {
            TextLine obj = _objRef as TextLine;
            TextLine old = _oldVal as TextLine;
            TextLine @new  = _newVal as TextLine;
            if (old.Text != @new.Text)
                obj.Text = @new.Text;
            if (old.Language != @new.Language)
                obj.Language = @new.Language;
        }
    }
}
