using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using FluentExplorer.Common;
using FluentExplorer.ViewModels;
using Newtonsoft.Json;
using Win32Interop.Shared.Models;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentExplorer.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LocalFolderPage : Page
    {
        public LocalFolderPage()
        {
            InitializeComponent();
        }

        private List<MenuFlyoutItemBase> GenerateMenu(string folderPath, List<MenuInfo> menuInfos)
        {
            return menuInfos.Select(it =>
            {
                if (string.IsNullOrEmpty(it.Title)) return new MenuFlyoutSeparator() as MenuFlyoutItemBase;

                var title = it.Title.Replace("&", "");
                var quickIndex = it.Title.IndexOf('&');
                KeyboardAccelerator keyboardAccelerator = null;
                if (quickIndex != -1)
                {
                    if (Enum.TryParse<VirtualKey>(it.Title[quickIndex + 1].ToString(), out var quickKey))
                    {
                        keyboardAccelerator = new KeyboardAccelerator
                        {
                            Key = quickKey
                        };
                        if (quickIndex > 0 && it.Title[quickIndex - 1] == '(')
                        {
                            var endIndex = it.Title.Substring(quickIndex).IndexOf(')');
                            if (endIndex != -1)
                            {
                                title = title.Remove(quickIndex - 1, endIndex + 1);
                            }
                        }
                    }
                }

                MenuFlyoutItemBase result = null;

                if (it.SubMenu.Any())
                {
                    var subItem = new MenuFlyoutSubItem
                    {
                        Text = title
                    };
                    GenerateMenu(folderPath, it.SubMenu).ForEach(m => subItem.Items.Add(m));
                    result = subItem;
                }
                else
                {
                    var item = new MenuFlyoutItem
                    {
                        Text = title,
                        Command = MenuCommand,
                        CommandParameter = new ContextMenuAction
                        {
                            Type = ContextMenuAction.ActionType.InvokeCommand,
                            MenuId = Convert.ToInt32(it.Id),
                            Path = folderPath,
                        }
                    };
                    
                    result = item;
                }

                if (keyboardAccelerator != null) result.KeyboardAccelerators.Add(keyboardAccelerator);

                return result;
            }).ToList();
        }

        public ICommand MenuCommand { get; } = new RelayCommand<ContextMenuAction>(it =>
        {
            App.Connection.SendMessageAsync(new ValueSet
            {
                {"type", "ContextMenu"},
                {"data", JsonConvert.SerializeObject(it)}
            }).FireAndForget();
        });

        protected override async void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            base.OnRightTapped(e);
            if (e.OriginalSource is FrameworkElement element)
                switch (element.DataContext)
                {
                    case IStorageItem storageItem:
                        if (!ItemsView.SelectedItems.Any() && !ItemsView.SelectedItems.Contains(storageItem))
                            ItemsView.SelectedItems.Add(storageItem);
                        //(storageItem as StorageFolder)
                        //storageItem.DateCreated

                        var result = await App.Connection.SendMessageAsync(new ValueSet
                        {
                            {"type", "ContextMenu"},
                            {
                                "data", JsonConvert.SerializeObject(new ContextMenuAction
                                {
                                    Type = ContextMenuAction.ActionType.ShowMenu,
                                    Path = storageItem.Path
                                })
                            }
                        });
                        var menu = JsonConvert.DeserializeObject<List<MenuInfo>>(result.Message["data"].ToString());
                        var menuFlyout = new MenuFlyout();
                        GenerateMenu(storageItem.Path, menu).ForEach(it => menuFlyout.Items.Add(it));
                        menuFlyout.ShowAt(this, e.GetPosition(this));
                        //FolderFlyout.ShowAt(this, e.GetPosition(this));
                        break;
                    //case StorageFile file:
                    //    break;
                }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            (DataContext as LocalFolderViewModel).OnLeave();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DataContext = e.Parameter;
            (DataContext as LocalFolderViewModel).OnEnter();
        }


        private void GridViewDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element && element.DataContext is IStorageItem item)
                switch (item)
                {
                    case StorageFolder folder:
                        Frame.Navigate(typeof(LocalFolderPage),
                            new LocalFolderViewModel(folder, (DataContext as LocalFolderViewModel).Path));
                        break;
                    case StorageFile file:
                        (DataContext as LocalFolderViewModel).OpenFile(file);
                        break;
                }
        }
    }
}