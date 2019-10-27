using Caliburn.Micro;
using DataViewer.Controllers;
using DataViewer.Interfaces;
using DataViewer.ViewModels;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.Windows;

namespace DataViewer
{
    public class Bootstrapper : BootstrapperBase
    {
        readonly SimpleContainer _container = new SimpleContainer();

        public Bootstrapper()
        {
            Initialize();
        }

        protected override object GetInstance(Type service, string key)
        {
            var instance = _container.GetInstance(service, key);
            if (instance != null)
                return instance;

            throw new InvalidOperationException("Could not locate any instances.");
        }

        protected override IEnumerable<object> GetAllInstances(Type service) => _container.GetAllInstances(service);

        protected override void BuildUp(object instance) => _container.BuildUp(instance);

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<IShellViewModel>(); // set MainViewModel to be the startup viewmodel

            GoogleTranslationCloudAuth();
        }

        protected override void Configure()
        {
            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.PerRequest<IHealDocumentController, HealDocumentController>();
            _container.PerRequest<IShellViewModel, ShellViewModel>();
        }

        /// <summary>
        /// Retrieves credentials necessary to use the cloud. 
        /// Requires GOOGLE_APPLICATION_CREDENTIALS environment variable to be set.
        /// <code></code>
        /// Official Google documentation: <a href="https://cloud.google.com/translate/docs/quickstart-client-libraries">https://cloud.google.com/translate/docs/quickstart-client-libraries</a>
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
