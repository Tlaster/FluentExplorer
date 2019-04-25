using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml.Controls;
using FluentExplorer.Controls;
using FluentExplorer.Views;

namespace FluentExplorer.ViewModels
{
    public class LocalFolderViewModel : FolderViewModelBase
    {
        public LocalFolderViewModel(StorageFolder folder, PathModel parentPathModel)
        {
            CurrentFolder = folder;
            Path = new PathModel(folder.DisplayName, folder.Path)
            {
                Parent = parentPathModel
            };
            Init();
        }

        public ObservableCollection<IStorageItem> StorageItems { get; } = new ObservableCollection<IStorageItem>();

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

        public override PathModel Path { get; }

        public override async Task<bool> TryGoUpAsync(Frame frame)
        {
            var parent = await CurrentFolder.GetParentAsync();
            if (parent != null)
            {
                frame.Navigate(typeof(LocalFolderPage), new LocalFolderViewModel(parent, Path?.Parent?.Parent));
                return true;
            }
            return false;
        }

        public void OpenFile(StorageFile file)
        {
            Launcher.LaunchFileAsync(file);
        }
    }
}
