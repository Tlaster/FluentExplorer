using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Microsoft.Toolkit.Uwp.UI.Animations;
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
        public RequestSubFolderPathModel(string name, string path, ImageSource icon)
        {
            Name = name;
            Path = path;
            Icon = icon;
        }

        public string Name { get; }
        public string Path { get; }
        public ImageSource Icon { get; }
    }

    public class PathModel : RequestSubFolderPathModel, INotifyPropertyChanged
    {
        private List<RequestSubFolderPathModel> _subFolders;

        public PathModel(string name, string path) : base(name, path, null)
        {
        }

        public PathModel Parent { get; set; }

        public List<RequestSubFolderPathModel> SubFolders
        {
            get => _subFolders;
            set
            {
                _subFolders = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed class PathView : Control
    {
        public static readonly DependencyProperty FolderImageSourceProperty = DependencyProperty.Register(
            nameof(FolderImageSource), typeof(ImageSource), typeof(PathView),
            new PropertyMetadata(default, OnPropertyChangedCallback));

        public static readonly DependencyProperty CurrentFolderPathProperty = DependencyProperty.Register(
            nameof(CurrentFolderPath), typeof(PathModel), typeof(PathView),
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

        public PathModel CurrentFolderPath
        {
            get => (PathModel) GetValue(CurrentFolderPathProperty);
            set => SetValue(CurrentFolderPathProperty, value);
        }

        public ImageSource FolderImageSource
        {
            get => (ImageSource) GetValue(FolderImageSourceProperty);
            set => SetValue(FolderImageSourceProperty, value);
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
                var secondaryButton = element.FindAscendantByName("SecondaryButton") as Button;
                var isPrimary = element.FindAscendantByName("PrimaryButton") != null;
                var isSecondary = secondaryButton != null;
                if (isPrimary)
                    RequestNavigation?.Invoke(this, item.Path);
                else if (isSecondary)
                {
                    if (item.SubFolders == null)
                    {
                        var requestSubFolder = RequestSubFolder;
                        if (requestSubFolder != null)
                        {

                            var args = new RequestSubFolderEventArgs(item.Path, list => { item.SubFolders = list; });
                            requestSubFolder.Invoke(this, args);
                        }
                    }

                    var flyoutRoot = (secondaryButton.Flyout as Flyout).Content as FrameworkElement;
                    var listView = flyoutRoot.FindDescendant<ListView>();
                    var tag = listView.Tag is bool viewTag && viewTag;
                    var content = secondaryButton.Content as FrameworkElement;
                    if (!tag)
                    {
                        var centerX = Convert.ToSingle(content.ActualWidth / 2D);
                        var centerY = Convert.ToSingle(content.ActualHeight / 2D);
                        void SubFolderItemClicked(object _, ItemClickEventArgs clickEventArgs)
                        {
                            secondaryButton.Flyout.Hide();
                            var clickedModel = clickEventArgs.ClickedItem as RequestSubFolderPathModel;
                            RequestNavigation?.Invoke(this, clickedModel.Path);
                        }

                        secondaryButton.Flyout.Opening += delegate
                        {
                            content.Rotate(90, centerX: centerX, centerY: centerY).Start();
                        };
                        secondaryButton.Flyout.Closing += delegate
                        {
                            content.Rotate(0, centerX: centerX, centerY: centerY).Start();
                        };
                        listView.ItemClick += SubFolderItemClicked;
                        listView.Tag = true;
                        content.Rotate(90, centerX: centerX, centerY: centerY).Start();
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
            if (_repeater == null) return;
            var result = new List<PathModel>();
            var path = CurrentFolderPath;
            while (path != null)
            {
                result.Add(path);
                path = path.Parent;
            }

            result.Reverse();
            _repeater.ItemsSource = result;
            _pathTextBox.Text = CurrentFolderPath.Path;
        }

        private void OnPathTextBoxOnLostFocus(object sender, RoutedEventArgs e)
        {
            _isTyping = false;
            SwitchToListBox();
        }

        private void OnPathTextBoxOnGotFocus(object sender, RoutedEventArgs e)
        {
            _isTyping = true;
            _pathTextBox.SelectionStart = _pathTextBox.Text.Length;
            _pathTextBox.SelectionLength = 0;
        }

        private void OnPathTextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void OnPathTextBoxOnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                _isTyping = false;
                RequestNavigation?.Invoke(this, _pathTextBox.Text);
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
            if (e.Property == FolderImageSourceProperty)
                (d as PathView)._folderImage.Source = e.NewValue as ImageSource;
            if (e.Property == CurrentFolderPathProperty) (d as PathView).UpdateListBox();
        }
    }
}