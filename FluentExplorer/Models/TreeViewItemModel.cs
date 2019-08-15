using FluentExplorer.Common;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace FluentExplorer.Models
{
    public class TreeViewItemModel
    {
        public ObservableCollection<TreeViewItemModel> SubFolders { get; } = new ObservableCollection<TreeViewItemModel>();

        public StorageFolder CurrentFolder { get; }

        public TreeViewItemModel(StorageFolder folder, bool withSecond)
        {
            CurrentFolder = folder;
            if (withSecond)
            {
                Task.Run(async () => await TryGetSubFolders(false)).FireAndForget();
            }
        }

        private bool _isExpanded;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                if (value)
                {
                    Parallel.ForEach(SubFolders, it =>
                    {
                        it.TryGetSubFolders(true).FireAndForget();
                    });
                }
            }
        }

        private bool _isLoading;
        private bool _isLoaded;

        private async Task TryGetSubFolders(bool withSecond)
        {
            if (_isLoading || _isLoaded)
            {
                return;
            }
            _isLoading = true;
            var subFolders = await CurrentFolder.GetFoldersAsync();
            SubFolders.AddAll(subFolders.Select(it => new TreeViewItemModel(it, withSecond)));
            _isLoaded = true;
            _isLoading = false;
        }
    }
}
