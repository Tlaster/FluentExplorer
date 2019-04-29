using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using FluentExplorer.Common;
using FluentExplorer.Controls;
using FluentExplorer.Models;

namespace FluentExplorer.ViewModels
{
    public class IndexViewModel : FolderViewModelBase
    {
        public static IndexViewModel Instance { get; } = new IndexViewModel();

        public ObservableCollection<DiskModel> Disks { get; } = new ObservableCollection<DiskModel>();

        private IndexViewModel()
        {
            Init();
        }

        private async void Init()
        {
            IsLoading = true;
            var list = new[] { "System.FreeSpace", "System.Capacity" };
            var folders = await Task.WhenAll(DriveInfo.GetDrives().Select(it => it.Name)
                .Select(async it => await StorageFolder.GetFolderFromPathAsync(it)));
            var properties = await Task.WhenAll(folders.Select(async it => new
            {
                size = await it.Properties.RetrievePropertiesAsync(list),
                folder = it
            }));
            var items = properties.Select(it => new DiskModel
            {
                StorageFolder = it.folder,
                FreeSpace = Convert.ToDouble(it.size["System.FreeSpace"]),
                Capacity = Convert.ToDouble(it.size["System.Capacity"])
            }).ToList();
            items.ForEach(it => Disks.Add(it));
            IsLoading = false;
        }

        public override PathModel Path { get; } = new PathModel(KnownFolders.DocumentsLibrary.Provider.DisplayName, "");
        public override async Task<bool> TryGoUpAsync(Frame frame)
        {
            return false;
        }
    }
}
