using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Mhyrenz_Interface.Features.Inventory.Controls;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Microsoft.Xaml.Behaviors;

namespace Mhyrenz_Interface.Features.Inventory.Behaviors
{
    public class InventoryDataGridDetailed : Behavior<DataGrid>
    {
        public static readonly DependencyProperty TabOwnerProperty =
            DependencyProperty.Register(
                nameof(TabOwner),
                typeof(InventoryTabItem),
                typeof(InventoryDataGridDetailed),
                new PropertyMetadata(null, OnTabOwnerChanged));

        private InventoryTabItem _currentTab;
        private InventoryDataGridViewModel _currentVm;

        public InventoryTabItem TabOwner
        {
            get => (InventoryTabItem)GetValue(TabOwnerProperty);
            set => SetValue(TabOwnerProperty, value);
        }

        private static void OnTabOwnerChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var behavior = (InventoryDataGridDetailed)d;

            if (behavior.AssociatedObject == null)
                return;

            behavior.ChangeTab(
                e.NewValue as InventoryTabItem,
                e.OldValue as InventoryTabItem);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += DataGrid_SelectionChanged;
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
            AssociatedObject.DataContextChanged += AssociatedObject_DataContextChanged;

            ChangeTab(TabOwner, null);
        }

        protected override void OnDetaching()
        {
            CleanupTab(_currentTab);
            CleanupVm(_currentVm);

            AssociatedObject.SelectionChanged -= DataGrid_SelectionChanged;
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            AssociatedObject.DataContextChanged -= AssociatedObject_DataContextChanged;

            _currentTab = null;
            _currentVm = null;

            base.OnDetaching();
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            ChangeTab(TabOwner, null);

            App.Current.Dispatcher.BeginInvoke(
                new Action(() => SelectRow()),
                DispatcherPriority.Background);
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            CleanupTab(_currentTab);
            CleanupVm(_currentVm);
        }

        private void AssociatedObject_DataContextChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            CleanupVm(e.OldValue as InventoryDataGridViewModel);

            _currentVm = e.NewValue as InventoryDataGridViewModel;

            SubscribeVm(_currentVm);
        }

        private void ChangeTab(
            InventoryTabItem newTab,
            InventoryTabItem oldTab)
        {
            if (ReferenceEquals(_currentTab, newTab))
                return;

            CleanupTab(_currentTab ?? oldTab);

            _currentTab = newTab;

            if (_currentTab == null)
                return;

            _currentTab.ColumnsChanged += InventoryTabItem_ColumnsChange;

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var vm = AssociatedObject.DataContext as InventoryDataGridViewModel;

                if (vm == null)
                    return;

                _currentVm = vm;
                SubscribeVm(vm);

                foreach (var col in AssociatedObject.Columns)
                {
                    BindingOperations.ClearBinding(
                        col,
                        DataGridColumn.VisibilityProperty);

                    BindingOperations.ClearBinding(
                        col,
                        DataGridColumn.DisplayIndexProperty);
                }

                _currentTab.LoadColumns(
                    AssociatedObject.Columns.Select(x => new ColumnInfo
                    {
                        Header = x.Header?.ToString(),
                        DisplayIndex = x.DisplayIndex,
                        IgnoreReorder =
                            InventoryDataGridColumn.GetIgnoreReorder(x),
                        IgnoreVisibilityToggle =
                            InventoryDataGridColumn.GetIgnoreVisibilityToggle(x),
                        PharmaColumn =
                            InventoryDataGridColumn.GetPharmaColumn(x),
                        PlaceOrderBound =
                            InventoryDataGridColumn.GetPlaceOrderBound(x)
                    }));

                foreach (var col in AssociatedObject.Columns)
                {
                    if (col.Header == null)
                        continue;

                    var key = col.Header.ToString();

                    BindingOperations.SetBinding(
                        col,
                        DataGridColumn.VisibilityProperty,
                        new Binding
                        {
                            Source = vm,
                            Path = new PropertyPath(
                                $"{nameof(InventoryDataGridViewModel.ColumnsSettings)}[{key}].{nameof(ColumnSettingViewModel.IsVisible)}"),
                            Converter = new BooleanToVisibilityConverter(),
                            FallbackValue = Visibility.Collapsed,
                            TargetNullValue = Visibility.Collapsed,
                        });

                    if (vm.ColumnsSettings.TryGetValue(key, out var setting))
                        col.DisplayIndex = setting.DisplayIndex;
                }

                SelectRow();

            }), DispatcherPriority.Loaded);
        }

        private void CleanupTab(InventoryTabItem tab)
        {
            if (tab == null)
                return;

            tab.ColumnsChanged -= InventoryTabItem_ColumnsChange;
        }

        private void SubscribeVm(InventoryDataGridViewModel vm)
        {
            if (vm == null)
                return;

            vm.SwitchSelectedItem -= InventoryDataGridSelect_SwitchSelectedItem;
            vm.SwitchSelectedItem += InventoryDataGridSelect_SwitchSelectedItem;

            vm.CommitEdits -= InventoryDataGridDetailed_CommitEdits;
            vm.CommitEdits += InventoryDataGridDetailed_CommitEdits;
        }

        private void CleanupVm(InventoryDataGridViewModel vm)
        {
            if (vm == null)
                return;

            vm.SwitchSelectedItem -= InventoryDataGridSelect_SwitchSelectedItem;
            vm.CommitEdits -= InventoryDataGridDetailed_CommitEdits;
        }

        private void InventoryTabItem_ColumnsChange()
        {
            var vm = AssociatedObject.DataContext as InventoryDataGridViewModel;

            if (vm == null)
                return;

            var columns = AssociatedObject.Columns
                .Select(col => new
                {
                    Column = col,
                    Key = col.Header as string
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                .Where(x => vm.ColumnsSettings.ContainsKey(x.Key))
                .Select(x => new
                {
                    x.Column,
                    Setting = vm.ColumnsSettings[x.Key]
                })
                .OrderBy(x => x.Setting.DisplayIndex)
                .ToList();

            var index = 0;

            foreach (var item in columns)
                item.Column.DisplayIndex = index++;
        }

        private void DataGrid_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            var vm = AssociatedObject.DataContext as InventoryDataGridViewModel;

            if (vm == null)
                return;

            vm.SelectedItems =
                AssociatedObject.SelectedItems
                    .Cast<ProductDataViewModel>();

            AssociatedObject.RowDetailsVisibilityMode =
                AssociatedObject.SelectedItems.Count <= 1
                    ? DataGridRowDetailsVisibilityMode.VisibleWhenSelected
                    : DataGridRowDetailsVisibilityMode.Collapsed;
        }

        private void InventoryDataGridSelect_SwitchSelectedItem()
        {
            SelectRow(isFromSwitch: true);
        }

        private void InventoryDataGridDetailed_CommitEdits()
        {
            AssociatedObject.CancelEdit(DataGridEditingUnit.Cell);
            AssociatedObject.CancelEdit(DataGridEditingUnit.Row);
        }

        private async void SelectRow(bool isFromSwitch = false)
        {
            await App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var vm =
                    AssociatedObject.DataContext
                        as InventoryDataGridViewModel;

                if (vm?.SelectionInfo == null)
                    return;

                if (!isFromSwitch && !vm.SelectionInfo.CanSelect)
                    return;

                var selectionMap =
                    vm.SelectionInfo.Items.ToHashSet();

                AssociatedObject.SelectedItems.Clear();

                foreach (var item in AssociatedObject.Items
                             .Cast<ProductDataViewModel>())
                {
                    if (selectionMap.Contains(item.Item.Id))
                        AssociatedObject.SelectedItems.Add(item);
                }

                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (AssociatedObject.SelectedItem != null)
                        AssociatedObject.ScrollIntoView(
                            AssociatedObject.SelectedItem);
                }), DispatcherPriority.ContextIdle);

                vm.SelectionInfo.CanSelect = false;

            }), DispatcherPriority.ContextIdle);
        }
    }

    public class ColumnInfo
    {
        public string Header { get; internal set; }
        public int DisplayIndex { get; internal set; }
        public bool IgnoreVisibilityToggle { get; internal set; }
        public bool PharmaColumn { get; internal set; }
        public bool IgnoreReorder { get; internal set; }
        public bool PlaceOrderBound { get; internal set; }
    }
}
