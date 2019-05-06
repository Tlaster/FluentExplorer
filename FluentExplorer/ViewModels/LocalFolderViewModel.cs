using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.System;
using Windows.UI.Xaml.Controls;
using FluentExplorer.Common;
using FluentExplorer.Controls;
using FluentExplorer.Views;
using Microsoft.Toolkit.Uwp.Helpers;

namespace FluentExplorer.ViewModels
{
    public class LocalFolderViewModel : FolderViewModelBase
    {
        private StorageItemQueryResult _query;

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

        public override PathModel Path { get; }

        private async void Init()
        {
            IsLoading = true;
            _query = CurrentFolder.CreateItemQuery();
            await UpdateItems();
            _query.ContentsChanged += QueryOnContentsChanged;

            IsLoading = false;
        }

        private async Task UpdateItems()
        {
            var items = await _query.GetItemsAsync();
            DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                StorageItems.Clear();
                foreach (var item in items)
                {
                    StorageItems.Add(item);
                }
            }).FireAndForget();
        }

        private void QueryOnContentsChanged(IStorageQueryResultBase sender, object args)
        {
            UpdateItems().FireAndForget();
        }

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
            Launcher.LaunchFileAsync(file).FireAndForget();
        }
    }
}