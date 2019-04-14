using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using FluentExplorer.Views;

namespace FluentExplorer.ViewModels
{
    public class LocalFolderViewModel : FolderViewModelBase
    {
        public LocalFolderViewModel(StorageFolder folder)
        {
            CurrentFolder = folder;
            Init();
        }

        public ObservableCollection<IStorageItem> StorageItems { get; set; } = new ObservableCollection<IStorageItem>();

        public StorageFolder CurrentFolder { get; }

        private async void Init()
        {
            IsLoading = true;
            var items = await CurrentFolder.GetItemsAsync();
            foreach (var item in items)
            {
                StorageItems.Add(item);
            }
            IsLoading = false;
        }

        public override async Task<bool> TryGoUpAsync(Frame frame)
        {
            var parent = await CurrentFolder.GetParentAsync();
            if (parent != null)
            {
                frame.Navigate(typeof(LocalFolderPage), new LocalFolderViewModel(parent));
                return true;
            }
            return false;
        }
    }
}
