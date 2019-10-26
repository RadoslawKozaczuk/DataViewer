using System.Windows;
using System.Windows.Threading;

namespace DataViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // unhandled exception handler
        void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("An unhandled exception just occurred: " + e.Exception.Message,
                "Unhandled Exception",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            e.Handled = true;
        }
    }
}
