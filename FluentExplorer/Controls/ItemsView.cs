using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using FluentExplorer.Common;
using Microsoft.Toolkit.Uwp.UI.Controls;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace FluentExplorer.Controls
{
    public sealed class ItemsView : Control
    {
        public enum Mode
        {
            Grid,
            List,
            DataGrid
        }

        public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register(
            nameof(SelectionMode), typeof(ListViewSelectionMode), typeof(ItemsView),
            new PropertyMetadata(ListViewSelectionMode.Extended));

        public static readonly DependencyProperty ListTemplateProperty = DependencyProperty.Register(
            nameof(ListTemplate), typeof(DataTemplate), typeof(ItemsView), new PropertyMetadata(default(DataTemplate)));

        public static readonly DependencyProperty GridTemplateProperty = DependencyProperty.Register(
            nameof(GridTemplate), typeof(DataTemplate), typeof(ItemsView), new PropertyMetadata(default(DataTemplate)));

        public static readonly DependencyProperty DataGridTemplateProperty = DependencyProperty.Register(
            nameof(DataGridTemplate), typeof(DataTemplate), typeof(ItemsView),
            new PropertyMetadata(default(DataTemplate)));

        public static readonly DependencyProperty DisplayModeProperty = DependencyProperty.Register(
            nameof(DisplayMode), typeof(Mode), typeof(ItemsView),
            new PropertyMetadata(Mode.List, OnPropertyChangedCallback));

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource), typeof(object), typeof(ItemsView), new PropertyMetadata(default));

        private readonly ObservableCollection<object> _selectedItems = new ObservableCollection<object>();

        private DataGrid _itemsDataGrid;
        private GridView _itemsGridView;
        private ListView _itemsListView;

        public ItemsView()
        {
            DefaultStyleKey = typeof(ItemsView);
            _selectedItems.CollectionChanged += OnSelectedItemsCollectionChanged;
            Columns.CollectionChanged += ColumnsOnCollectionChanged;
        }

        public ObservableCollection<DataGridColumn> Columns { get; } = new ObservableCollection<DataGridColumn>();

        public IList<object> SelectedItems => _selectedItems;

        public ListViewSelectionMode SelectionMode
        {
            get => (ListViewSelectionMode) GetValue(SelectionModeProperty);
            set => SetValue(SelectionModeProperty, value);
        }

        public DataTemplate ListTemplate
        {
            get => (DataTemplate) GetValue(ListTemplateProperty);
            set => SetValue(ListTemplateProperty, value);
        }

        public DataTemplate GridTemplate
        {
            get => (DataTemplate) GetValue(GridTemplateProperty);
            set => SetValue(GridTemplateProperty, value);
        }

        public DataTemplate DataGridTemplate
        {
            get => (DataTemplate) GetValue(DataGridTemplateProperty);
            set => SetValue(DataGridTemplateProperty, value);
        }

        public Mode DisplayMode
        {
            get => (Mode) GetValue(DisplayModeProperty);
            set => SetValue(DisplayModeProperty, value);
        }

        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        private void ColumnsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_itemsDataGrid == null) return;

            _itemsDataGrid.Columns.AddAll(e.NewStartingIndex, e.NewItems);
            _itemsDataGrid.Columns.RemoveAll(e.OldStartingIndex, e.OldItems);
        }

        private static void OnPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == DisplayModeProperty)
                (d as ItemsView).UpdateDisplayMode(e.NewValue is Mode mode ? mode : Mode.Grid);
        }

        private void UpdateDisplayMode(Mode mode)
        {
            if (_itemsDataGrid == null || _itemsGridView == null || _itemsListView == null) return;

            _itemsDataGrid.Visibility = Visibility.Collapsed;
            _itemsGridView.Visibility = Visibility.Collapsed;
            _itemsListView.Visibility = Visibility.Collapsed;

            switch (mode)
            {
                case Mode.Grid:
                    _itemsGridView.Visibility = Visibility.Visible;
                    break;
                case Mode.List:
                    _itemsListView.Visibility = Visibility.Visible;
                    break;
                case Mode.DataGrid:
                    _itemsDataGrid.Visibility = Visibility.Visible;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            base.OnTapped(e);
            if (e.OriginalSource is FrameworkElement element && ItemsSource is IEnumerable items)
            {
                var type = element.DataContext?.GetType();
                var list = items.Cast<object>().ToList();
                var targetType = list.FirstOrDefault()?.GetType();
                if (type != null && targetType != null && type != targetType)
                {
                    _selectedItems.Clear();
                    e.Handled = true;
                }
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _itemsGridView = GetTemplateChild("ItemsGridView") as GridView;
            _itemsListView = GetTemplateChild("ItemsListView") as ListView;
            _itemsDataGrid = GetTemplateChild("ItemsDataGrid") as DataGrid;
            _itemsGridView.SelectionChanged += OnSelectionChanged;
            _itemsListView.SelectionChanged += OnSelectionChanged;
            _itemsDataGrid.SelectionChanged += OnSelectionChanged;
            UpdateDisplayMode(DisplayMode);
            _itemsDataGrid.Columns.AddAll(Columns);
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (sender)
            {
                case GridView gridView:
                    if (gridView.IsVisible())
                    {
                        _itemsListView.SelectedItems.AddAll(e.AddedItems);
                        _itemsListView.SelectedItems.RemoveAll(e.RemovedItems);
                        _itemsDataGrid.SelectedItems.AddAll(e.AddedItems);
                        _itemsDataGrid.SelectedItems.RemoveAll(e.RemovedItems);
                    }

                    break;
                case ListView listView:
                    if (listView.IsVisible())
                    {
                        _itemsGridView.SelectedItems.AddAll(e.AddedItems);
                        _itemsGridView.SelectedItems.RemoveAll(e.RemovedItems);
                        _itemsDataGrid.SelectedItems.AddAll(e.AddedItems);
                        _itemsDataGrid.SelectedItems.RemoveAll(e.RemovedItems);
                    }

                    break;
                case DataGrid dataGrid:
                    if (dataGrid.IsVisible())
                    {
                        _itemsListView.SelectedItems.AddAll(e.AddedItems);
                        _itemsListView.SelectedItems.RemoveAll(e.RemovedItems);
                        _itemsDataGrid.SelectedItems.AddAll(e.AddedItems);
                        _itemsDataGrid.SelectedItems.RemoveAll(e.RemovedItems);
                    }

                    break;
            }

            _selectedItems.AddAll(e.AddedItems);
            _selectedItems.RemoveAll(e.RemovedItems);
        }

        private void OnSelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_itemsListView.IsVisible())
            {
                _itemsListView.SelectedItems.AddAll(e.NewItems?.Cast<object>());
                _itemsListView.SelectedItems.RemoveAll(e.OldItems?.Cast<object>());
            }
            else if (_itemsGridView.IsVisible())
            {
                _itemsGridView.SelectedItems.AddAll(e.NewItems?.Cast<object>());
                _itemsGridView.SelectedItems.RemoveAll(e.OldItems?.Cast<object>());
            }
            else if (_itemsDataGrid.IsVisible())
            {
                _itemsDataGrid.SelectedItems.AddAll(e.NewItems);
                _itemsDataGrid.SelectedItems.RemoveAll(e.OldItems);
            }
        }
    }
}