using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace FluentExplorer.Common
{
    public static class PermissionHelper
    {
        public static async Task<bool> HasFullAccess()
        {
            try
            {
                var infos = DriveInfo.GetDrives();
                await StorageFolder.GetFolderFromPathAsync(infos.FirstOrDefault().Name);
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }

            return true;
        }

        public static async Task RequestPermission()
        {
            var dialog = new MessageDialog("You need to enable file system access on your settings before using the application", "Request permission");
            dialog.Commands.Add(new UICommand("Grant", command =>
            {
                Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess"));
            }));
            dialog.Commands.Add(new UICommand("Cancel"));
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            await dialog.ShowAsync();
        }
    }
}
