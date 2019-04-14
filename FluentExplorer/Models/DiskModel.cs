using Windows.Storage;
using Humanizer;

namespace FluentExplorer.Models
{
    public class DiskModel
    {
        public StorageFolder StorageFolder { get; set; }
        public double Capacity { get; set; }
        public double FreeSpace { get; set; }
        public string CapacityHuman => Capacity.Bytes().ToString("0.0");
        public string FreeSpaceHuman => FreeSpace.Bytes().ToString("0.0");
        public double Current => Capacity - FreeSpace;
        public string CurrentHuman => Current.Bytes().ToString("0.0");
    }
}