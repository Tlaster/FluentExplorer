using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using FluentExplorer.Views;
using System;

namespace FluentExplorer
{
    sealed partial class App : Application
    {
        public static AppServiceConnection Connection;
        private BackgroundTaskDeferral appServiceDeferral;

        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        private async void OnCloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            if (Connection != null)
            {
                var def = e.GetDeferral();
                await Connection.SendMessageAsync(new ValueSet
                {
                    {"type", "Close"}
                });
                def.Complete();
            }

        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            var rootView = Window.Current.Content as RootView;
            if (rootView == null)
            {
                rootView = new RootView();

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                Window.Current.Content = rootView;
            }

            if (e.PrelaunchActivated == false) Window.Current.Activate();
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequested;
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);

            if (args.TaskInstance.TriggerDetails is AppServiceTriggerDetails)
            {
                appServiceDeferral = args.TaskInstance.GetDeferral();
                args.TaskInstance.Canceled += OnTaskCanceled;

                var details = args.TaskInstance.TriggerDetails as AppServiceTriggerDetails;
                Connection = details.AppServiceConnection;
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            appServiceDeferral?.Complete();
        }
    }
}