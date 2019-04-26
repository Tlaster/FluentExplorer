using System;
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
    public sealed partial class LocalFolderPage : Page
    {
        public LocalFolderPage()
        {
            this.InitializeComponent();
        }

        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            base.OnTapped(e);
            if (e.OriginalSource is FrameworkElement element && !(element.DataContext is IStorageItem))
            {
                ItemsGridView.SelectedItems.Clear();
            }
        }

        protected override void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            base.OnRightTapped(e);
            if (e.OriginalSource is FrameworkElement element)
            {
                switch (element.DataContext)
                {
                    case StorageFolder folder:
                        
                        if (!ItemsGridView.SelectedItems.Any() && !ItemsGridView.SelectedItems.Contains(folder))
                        {
                            ItemsGridView.SelectedItems.Add(folder);
                        }
                        FolderFlyout.ShowAt(this, e.GetPosition(this));
                        break;
                    case StorageFile file:
                        break;
                    default:
                        
                        break;
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.DataContext = e.Parameter;
        }


        private void GridViewDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element && element.DataContext is IStorageItem item)
            {
                switch (item)
                {
                    case StorageFolder folder:
                        Frame.Navigate(typeof(LocalFolderPage), new LocalFolderViewModel(folder, (DataContext as LocalFolderViewModel).Path));
                        break;
                    case StorageFile file:
                        (DataContext as LocalFolderViewModel).OpenFile(file);
                        break;
                }
            }
        }
    }
}
