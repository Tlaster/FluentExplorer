﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using FluentExplorer.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentExplorer.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StoragePage : Page
    {
        public StoragePage()
        {
            this.InitializeComponent();
        }

        private void GobackClick(object sender, RoutedEventArgs e)
        {
            StorageNavigationFrame.GoBack();
        }

        private async void GoupClick(object sender, RoutedEventArgs e)
        {
            if (StorageNavigationFrame.Content is FrameworkElement element && element.DataContext is FolderViewModelBase viewModel)
            {
                if (!await viewModel.TryGoUpAsync(StorageNavigationFrame))
                {
                    StorageNavigationFrame.Navigate(typeof(IndexPage));
                }
            }
        }

        private void StorageNavigationFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            if (e.Parameter is FolderViewModelBase viewModel)
            {
                StoragePathView.Path = viewModel.Path;
            }
            else if (e.SourcePageType == typeof(IndexPage))
            {
                StoragePathView.Path = "";
            }
        }

        private void StoragePathView_OnRequestNavigation(object sender, string e)
        {
            
        }
    }
}
