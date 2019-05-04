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
using Newtonsoft.Json;
using Win32Interop.Shared.Models;

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

        protected override async void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            base.OnRightTapped(e);
            if (e.OriginalSource is FrameworkElement element)
            {
                switch (element.DataContext)
                {
                    case IStorageItem folder:
                        if (!ItemsGridView.SelectedItems.Any() && !ItemsGridView.SelectedItems.Contains(folder))
                        {
                            ItemsGridView.SelectedItems.Add(folder);
                        }

                        var result = await App.Connection.SendMessageAsync(new ValueSet()
                        {
                            {"type", "ContextMenu"},
                            {"data", folder.Path}
                        });
                        var menu = JsonConvert.DeserializeObject<List<MenuInfo>>(result.Message["data"].ToString());
                        var menuFlyout = new MenuFlyout();
                        menu.ForEach(it =>
                        {
                            if (string.IsNullOrEmpty(it.Title))
                            {
                                menuFlyout.Items.Add(new MenuFlyoutSeparator());
                            }
                            else if (it.SubMenu.Any())
                            {
                                //TODO
                                var item = new MenuFlyoutSubItem()
                                {
                                    Text = it.Title.Replace("&", "")
                                };

                                it.SubMenu.Select(m => new MenuFlyoutItem()
                                {
                                    Text = m.Title.Replace("&", "")
                                }).ToList().ForEach(m => item.Items.Add(m));
                                menuFlyout.Items.Add(item);
                            }
                            else
                            {
                                menuFlyout.Items.Add(new MenuFlyoutItem()
                                {
                                    Text = it.Title.Replace("&", "")
                                });
                            }
                        });
                        menuFlyout.ShowAt(this, e.GetPosition(this));
                        //FolderFlyout.ShowAt(this, e.GetPosition(this));
                        break;
                    //case StorageFile file:
                    //    break;
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
