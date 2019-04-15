using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using FluentExplorer.Common;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Microsoft.UI.Xaml.Controls;

namespace FluentExplorer.Controls
{
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

        private Button _expandButton;
        private Image _folderImage;
        private bool _isTyping;
        private ItemsRepeater _repeater;
        private TextBox _pathTextBox;
        private Button _refreshButton;

        public PathView()
        {
            DefaultStyleKey = typeof(PathView);
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
            (GetTemplateChild("ToggleGrid") as Grid).Tapped += OnToggleGridTapped;
            
            UpdateListBox();
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
                path?.Split(System.IO.Path.DirectorySeparatorChar)?.Where(it => !string.IsNullOrEmpty(it)).ToList() ??
                new List<string>();
            if (!string.IsNullOrEmpty(RootName)) result.Insert(0, RootName);

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
        }

        private void OnPathChanged(string newPath)
        {
            if (_isTyping) return;
            UpdateListBox();
            if (_pathTextBox != null) _pathTextBox.Text = newPath;
        }
    }
}