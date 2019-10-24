using Caliburn.Micro;
using DataViewer.ViewModels;
using System.Windows;

namespace DataViewer
{
    public class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainViewModel>(); // set MainViewModel to be the startup viewmodel
        }
    }
}
