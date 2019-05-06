using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
            _query = CurrentFolder.CreateItemQuery();
        }

        public ObservableCollection<IStorageItem> StorageItems { get; } = new ObservableCollection<IStorageItem>();

        public StorageFolder CurrentFolder { get; }

        public override PathModel Path { get; }


        private async Task UpdateItems()
        {
            if (IsLoading)
            {
                return;
            }

            IsLoading = true;
            var items = await _query.GetItemsAsync();
            if (items.SequenceEqual(StorageItems, new GenericCompare<IStorageItem>(item => item.Name)))
            {
                IsLoading = false;
                return;
            }
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                StorageItems.Clear();
                foreach (var item in items)
                {
                    StorageItems.Add(item);
                }
            });
            IsLoading = false;
        }

        private void QueryOnContentsChanged(IStorageQueryResultBase sender, object args)
        {
            UpdateItems().FireAndForget();
        }

        public void OnLeave()
        {
            _query.ContentsChanged -= QueryOnContentsChanged;
        }

        public void OnEnter()
        {
            _query.ContentsChanged += QueryOnContentsChanged;
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