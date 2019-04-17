using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using FluentExplorer.Common;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Microsoft.UI.Xaml.Controls;
using SplitButton = Microsoft.UI.Xaml.Controls.SplitButton;

namespace FluentExplorer.Controls
{
    public class RequestSubFolderEventArgs : EventArgs
    {
        public RequestSubFolderEventArgs(string path, Action<List<RequestSubFolderPathModel>> callback)
        {
            Path = path;
            Callback = callback;
        }

        public string Path { get; }
        public Action<List<RequestSubFolderPathModel>> Callback { get; }
    }

    public class RequestSubFolderPathModel
    {
        public RequestSubFolderPathModel(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public string Name { get; }
        public string Path { get; }

    }

    public class PathModel : RequestSubFolderPathModel, INotifyPropertyChanged
    {
        private List<RequestSubFolderPathModel> _subFolders;

        public List<RequestSubFolderPathModel> SubFolders
        {
            get => _subFolders;
            set
            {
                _subFolders = value;
                OnPropertyChanged();
            }
        }

        public PathModel(string name, string path) : base(name, path)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed class PathView : Control
    {
        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
            nameof(Path), typeof(string), typeof(PathView), new PropertyMetadata(default, OnPropertyChangedCallback));

        public static readonly DependencyProperty RootNameProperty = DependencyProperty.Register(
            nameof(RootName), typeof(string), typeof(PathView),
            new PropertyMetadata(default, OnPropertyChangedCallback));

        public static readonly DependencyProperty RootPathProperty = DependencyProperty.Register(
            nameof(RootPath), typeof(string), typeof(PathView),
            new PropertyMetadata(default, OnPropertyChangedCallback));

        public static readonly DependencyProperty FolderImageSourceProperty = DependencyProperty.Register(
            nameof(FolderImageSource), typeof(ImageSource), typeof(PathView),
            new PropertyMetadata(default, OnPropertyChangedCallback));

        private Button _expandButton;
        private Image _folderImage;
        private bool _isTyping;
        private TextBox _pathTextBox;
        private Button _refreshButton;
        private ItemsRepeater _repeater;

        public PathView()
        {
            DefaultStyleKey = typeof(PathView);
        }

        public ImageSource FolderImageSource
        {
            get => (ImageSource) GetValue(FolderImageSourceProperty);
            set => SetValue(FolderImageSourceProperty, value);
        }

        public string RootName
        {
            get => (string) GetValue(RootNameProperty);
            set => SetValue(RootNameProperty, value);
        }

        public string RootPath
        {
            get => (string) GetValue(RootPathProperty);
            set => SetValue(RootPathProperty, value);
        }

        public string Path
        {
            get => (string) GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }

        public event EventHandler<string> RequestNavigation;
        public event EventHandler RequestRefresh;
        public event EventHandler<RequestSubFolderEventArgs> RequestSubFolder;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _pathTextBox = GetTemplateChild("PathViewTextBox") as TextBox;
            _pathTextBox.TextChanged += OnPathTextBoxOnTextChanged;
            _pathTextBox.LostFocus += OnPathTextBoxOnLostFocus;
            _pathTextBox.GotFocus += OnPathTextBoxOnGotFocus;
            _pathTextBox.KeyDown += OnPathTextBoxOnKeyDown;
            _folderImage = GetTemplateChild("PathViewFolderImage") as Image;
            _expandButton = GetTemplateChild("PathViewExpandButton") as Button;
            _refreshButton = GetTemplateChild("PathViewRefreshButton") as Button;
            _refreshButton.Click += OnRefreshButtonOnClick;
            _repeater = GetTemplateChild("PathViewListBox") as ItemsRepeater;
            _repeater.Tapped += OnRepeaterTapped;
            (GetTemplateChild("ToggleGrid") as Grid).Tapped += OnToggleGridTapped;
            UpdateListBox();
        }

        private void OnRepeaterTapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element && element.DataContext is PathModel item)
            {
                var root = element.FindAscendant<SplitButton>();
                var isPrimary = element.FindAscendantByName("PrimaryButton") != null;
                var isSecondary = element.FindAscendantByName("SecondaryButton") != null;
                if (isPrimary)
                {
                    RequestNavigation?.Invoke(this, item.Path);
                }
                else if (isSecondary)
                {
                    if (item.SubFolders == null)
                    {
                        var requestSubFolder = RequestSubFolder;
                        if (requestSubFolder != null)
                        {
                            var flyoutRoot = (root.Flyout as Flyout).Content as FrameworkElement;
                            var listView = flyoutRoot.FindDescendant<ListView>();
                            var tag = listView.Tag is bool viewTag && viewTag;
                            if (!tag)
                            {

                                void SubFolderItemClicked(object _, ItemClickEventArgs clickEventArgs)
                                {
                                    root.Flyout.Hide();
                                    var clickedModel = clickEventArgs.ClickedItem as RequestSubFolderPathModel;
                                    RequestNavigation?.Invoke(this, clickedModel.Path);
                                }
                                listView.ItemClick += SubFolderItemClicked;
                                listView.Tag = true;
                            }
                            var args = new RequestSubFolderEventArgs(item.Path, list =>
                            {
                                item.SubFolders = list;
                            });
                            requestSubFolder.Invoke(this, args);
                        }
                    }
                }
            }
        }

        private void OnToggleGridTapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchToTextBox();
        }

        private void UpdateListBox()
        {
            var path = Path;
            if (!string.IsNullOrEmpty(RootPath) || !string.IsNullOrEmpty(RootName)) path = path.TrimStart(RootPath);

            var result =
                path?.Split(System.IO.Path.DirectorySeparatorChar)
                    ?.Where(it => !string.IsNullOrEmpty(it))
                    ?.Select(it =>
                        new PathModel(it, Path.Substring(0, Path.IndexOf(it, StringComparison.Ordinal) + it.Length)))
                    ?.ToList() ??
                new List<PathModel>();
            if (!string.IsNullOrEmpty(RootName)) result.Insert(0, new PathModel(RootName, RootPath));

            if (_repeater != null) _repeater.ItemsSource = result;
        }

        private void OnPathTextBoxOnLostFocus(object sender, RoutedEventArgs e)
        {
            _isTyping = false;
            SwitchToListBox();
        }

        private void OnPathTextBoxOnGotFocus(object sender, RoutedEventArgs e)
        {
            _isTyping = true;
        }

        private void OnPathTextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            Path = _pathTextBox.Text;
        }

        private void OnPathTextBoxOnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                RequestNavigation?.Invoke(this, Path);
                SwitchToListBox();
            }
        }

        private void SwitchToTextBox()
        {
            _repeater.Visibility = Visibility.Collapsed;
            _pathTextBox.Visibility = Visibility.Visible;
            _pathTextBox.Focus(FocusState.Programmatic);
        }

        private void SwitchToListBox()
        {
            _pathTextBox.Visibility = Visibility.Collapsed;
            _repeater.Visibility = Visibility.Visible;
        }

        private void OnRefreshButtonOnClick(object sender, RoutedEventArgs e)
        {
            RequestRefresh?.Invoke(this, EventArgs.Empty);
        }

        private static void OnPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == PathProperty) (d as PathView).OnPathChanged(e.NewValue as string);
            if (e.Property == RootNameProperty || e.Property == RootPathProperty) (d as PathView).UpdateListBox();
            if (e.Property == FolderImageSourceProperty)
                (d as PathView)._folderImage.Source = e.NewValue as ImageSource;
        }

        private void OnPathChanged(string newPath)
        {
            if (_isTyping) return;
            UpdateListBox();
            if (_pathTextBox != null) _pathTextBox.Text = newPath;
        }
    }
}