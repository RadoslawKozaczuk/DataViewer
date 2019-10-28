using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace DataViewer.Input
{
    // this class is needed to support key binding
    public class InputBindings
    {
        readonly InputBindingCollection _inputBindings;
        readonly Stack<KeyBinding> _stash;

        public InputBindings(Window bindingsOwner)
        {
            _inputBindings = bindingsOwner.InputBindings;
            _stash = new Stack<KeyBinding>();
        }

        public void RegisterCommands(IEnumerable<InputBindingCommand> inputBindingCommands)
        {
            foreach (var inputBindingCommand in inputBindingCommands)
            {
                var binding = new KeyBinding(inputBindingCommand, inputBindingCommand.GestureKey, inputBindingCommand.GestureModifier);

                _stash.Push(binding);
                _inputBindings.Add(binding);
            }
        }

        public void DeregisterCommands()
        {
            if (_inputBindings == null)
                return;

            foreach (var keyBinding in _stash)
                _inputBindings.Remove(keyBinding);
        }
    }
}
