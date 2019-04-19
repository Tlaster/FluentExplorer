﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using FluentExplorer.Controls;
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
                StoragePathView.CurrentFolderPath = viewModel.Path;
            }
            else if ((e.Content as FrameworkElement)?.DataContext is FolderViewModelBase vm)
            {
                StoragePathView.CurrentFolderPath = vm.Path;
            }
            //else if (e.SourcePageType == typeof(IndexPage))
            //{
            //    StoragePathView.CurrentFolderPath = null;
            //}
        }

        private async void StoragePathView_OnRequestNavigation(object sender, string e)
        {
            if (string.IsNullOrEmpty(e))
            {
                StorageNavigationFrame.Navigate(typeof(IndexPage));
            }
            else
            {
                try
                {
                    StorageNavigationFrame.Navigate(typeof(LocalFolderPage),
                        new LocalFolderViewModel(await StorageFolder.GetFolderFromPathAsync(e), await GeneratePath(e)));
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message);
                    Debug.WriteLine(exception.StackTrace);
                }
            }
        }

        private async Task<PathModel> GeneratePath(string path)
        {
            var folder = await StorageFolder.GetFolderFromPathAsync(path);
            folder = await folder.GetParentAsync();
            if (folder == null)
            {
                return IndexViewModel.Instance.Path;
            }
            var pathModel = new PathModel(folder.DisplayName, folder.Path);
            var temp = pathModel;
            while (folder != null)
            {
                folder = await folder.GetParentAsync();
                if (folder != null)
                {
                    temp.Parent = new PathModel(folder.DisplayName, folder.Path);
                    temp = temp.Parent;
                }
            }

            temp.Parent = IndexViewModel.Instance.Path;
            return pathModel;
        }

        private async void StoragePathView_OnRequestSubFolder(object sender, RequestSubFolderEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Path))
            {
                e.Callback.Invoke(IndexViewModel.Instance.Disks.Select(it => new RequestSubFolderPathModel(it.StorageFolder.DisplayName, it.StorageFolder.Path, new SvgImageSource(new Uri("ms-appx:///Assets/HardDrive.svg")))).ToList());
            }
            else
            {
                var folder = await StorageFolder.GetFolderFromPathAsync(e.Path);
                var subFolders = await folder.GetFoldersAsync();
                e.Callback.Invoke(subFolders.Select(it => new RequestSubFolderPathModel(it.Name, it.Path, new SvgImageSource(new Uri("ms-appx:///Assets/Folder.svg")))).ToList());
            }
        }
    }
}
