using System;
using System.Windows.Input;

namespace DataViewer.Input
{
    public class InputBindingCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public Key GestureKey { get; set; }

        public ModifierKeys GestureModifier { get; set; }

        public MouseAction MouseGesture { get; set; }

        readonly Action<object> _executeDelegate;
        Func<object, bool> _canExecutePredicate;

        public InputBindingCommand(Action executeDelegate)
        {
            _executeDelegate = x => executeDelegate();
            _canExecutePredicate = x => true;
        }

        public InputBindingCommand(Action<object> executeDelegate)
        {
            _executeDelegate = executeDelegate;
            _canExecutePredicate = x => true;
        }

        public bool CanExecute(object parameter) => _canExecutePredicate(parameter);

        public void Execute(object parameter) => _executeDelegate(parameter);

        public InputBindingCommand If(Func<bool> canExecutePredicate)
        {
            _canExecutePredicate = x => canExecutePredicate();
            return this;
        }

        public InputBindingCommand If(Func<object, bool> canExecutePredicate)
        {
            _canExecutePredicate = canExecutePredicate;
            return this;
        }
    }
}
