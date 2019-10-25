using Caliburn.Micro;
using DataViewer.Input;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace DataViewer
{
    public abstract class ViewModelBase : Screen, INotifyPropertyChanged
    {
        InputBindings _inputBindings;

        public void Deactivate(bool close) => OnDeactivate(close);

        protected IWindowManager WindowManager { get; } = new WindowManager();

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            var window = (view as FrameworkElement).GetWindow();
            _inputBindings = new InputBindings(window);
            _inputBindings.RegisterCommands(GetInputBindingCommands());
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);

            _inputBindings.DeregisterCommands();
        }

        protected virtual IEnumerable<InputBindingCommand> GetInputBindingCommands()
        {
            yield break;
        }
    }
}
