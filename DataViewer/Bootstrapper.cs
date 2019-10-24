using Caliburn.Micro;
using DataViewer.ViewModels;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
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
            GoogleTranslationCloudAuth();
        }

        /// <summary>
        /// Retrieves credentials necessary to use the cloud. 
        /// Requires GOOGLE_APPLICATION_CREDENTIALS environment variable to be set.
        /// </summary>
        void GoogleTranslationCloudAuth()
        {
            // If you don't specify credentials when constructing the client, the
            // client library will look for credentials in the environment.
            var credential = GoogleCredential.GetApplicationDefault();
            using var storage = StorageClient.Create(credential);
        }
    }
}
