using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using FluentExplorer.Controls;

namespace FluentExplorer.ViewModels
{
    public abstract class FolderViewModelBase : ViewModelBase
    {
        private ItemsView.Mode _displayMode = ItemsView.Mode.Grid;

        public ItemsView.Mode DisplayMode
        {
            get => _displayMode;
            set
            {
                _displayMode = value;
                OnPropertyChanged();
            }
        }

        public abstract PathModel Path { get; }
        public abstract Task<bool> TryGoUpAsync(Frame frame);
    }

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotLoading));
            }
        }

        public bool IsNotLoading => !IsLoading;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
