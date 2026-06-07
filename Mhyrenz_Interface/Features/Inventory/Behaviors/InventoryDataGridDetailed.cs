using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Features.Inventory.Controls;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Shared.Columns;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Xaml.Behaviors;

namespace Mhyrenz_Interface.Features.Inventory.Behaviors
{
    public class InventoryDataGridDetailed : Behavior<DataGrid>
    {
        public delegate void TabChangedEvent(InventoryTabItem newTab, InventoryTabItem oldTab);

        public static readonly DependencyProperty TabOwnerProperty =
            DependencyProperty.Register(
                "TabOwner",
                typeof(InventoryTabItem),
                typeof(InventoryDataGridDetailed),
                new PropertyMetadata(null, OnTabOwnerChanged));

        public static event TabChangedEvent TabChanged;

        public InventoryTabItem TabOwner
        {
            get => (InventoryTabItem)GetValue(TabOwnerProperty);
            set => SetValue(TabOwnerProperty, value);
        }

        private static void OnTabOwnerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (InventoryDataGridDetailed)d;

            if (!(e.NewValue is InventoryTabItem newTab) || behavior.AssociatedObject == null)
                return;

            //behavior.InventoryDataGridDetailed_TabChanged(tab);
            TabChanged?.Invoke(newTab, e.OldValue as InventoryTabItem);
        }

        protected override void OnAttached()
        {
            TabChanged += InventoryDataGridDetailed_TabChanged;
            TabChanged += InventoryDataGridSelect;

            AssociatedObject.SelectionChanged += DataGrid_SelectionChanged;
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = AssociatedObject.DataContext.CastTo<InventoryDataGridViewModel>();

            vm.CommitEdits -= InventoryDataGridDetailed_CommitEdits;
            vm.CommitEdits += InventoryDataGridDetailed_CommitEdits;

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                SelectRow();
            }), DispatcherPriority.Background);
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            TabOwner.ColumnsChanged -= InventoryTabItem_ColumnsChange;

            TabChanged -= InventoryDataGridDetailed_TabChanged;
            TabChanged -= InventoryDataGridSelect;

            AssociatedObject.SelectionChanged -= DataGrid_SelectionChanged;
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;

            var vm = AssociatedObject.DataContext.CastTo<InventoryDataGridViewModel>();
            vm.SwitchSelectedItem -= InventoryDataGridSelect_SwitchSelectedItem;
            vm.CommitEdits -= InventoryDataGridDetailed_CommitEdits;
        }

        private void InventoryDataGridDetailed_TabChanged(InventoryTabItem newTab, InventoryTabItem oldTab)
        {
            newTab.ColumnsChanged += InventoryTabItem_ColumnsChange;

            if (oldTab != null) 
                oldTab.ColumnsChanged -= InventoryTabItem_ColumnsChange;

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var vm = AssociatedObject.DataContext.CastTo<InventoryDataGridViewModel>();

                // Clear stale bindings from the previous tab BEFORE touching LoadColumns
                foreach (var col in AssociatedObject.Columns)
                {
                    BindingOperations.ClearBinding(col, DataGridColumn.VisibilityProperty);
                    BindingOperations.ClearBinding(col, DataGridColumn.DisplayIndexProperty);
                }

                newTab.LoadColumns(AssociatedObject.Columns.Select(x => new ColumnInfo
                {
                    Header = x.Header?.ToString(),
                    DisplayIndex = x.DisplayIndex,
                    IgnoreReorder = InventoryDataGridColumn.GetIgnoreReorder(x),
                    IgnoreVisibilityToggle = InventoryDataGridColumn.GetIgnoreVisibilityToggle(x),
                }));

                foreach (var col in AssociatedObject.Columns)
                {
                    if (col.Header == null) continue;

                    BindingOperations.SetBinding(col, DataGridColumn.VisibilityProperty, new Binding
                    {
                        Source = vm,
                        Path = new PropertyPath($"{nameof(InventoryDataGridViewModel.ColumnsSettings)}[{col.Header}].{nameof(ColumnSettingViewModel.IsVisible)}"),
                        Converter = new BooleanToVisibilityConverter(),
                    });

                    col.DisplayIndex = vm.ColumnsSettings[(string)col.Header].DisplayIndex;
                }
            }));
        }

        private void InventoryTabItem_ColumnsChange()
        {
            var vm = AssociatedObject.DataContext.CastTo<InventoryDataGridViewModel>();
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

            int index = 0;

            foreach (var item in columns)
            {
                item.Column.DisplayIndex = index++;
            }
        }

        private void InventoryDataGridSelect(InventoryTabItem newTab, InventoryTabItem oldTab)
        {
            newTab.ContentViewModel.SwitchSelectedItem += InventoryDataGridSelect_SwitchSelectedItem;
            AssociatedObject.DataContextChanged += AssociatedObject_DataContextChanged;
        }

        private void AssociatedObject_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AssociatedObject.DataContextChanged -= AssociatedObject_DataContextChanged;

            e.OldValue.CastTo<InventoryDataGridViewModel>()
                .SwitchSelectedItem -= InventoryDataGridSelect_SwitchSelectedItem;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = (InventoryDataGridViewModel)AssociatedObject.DataContext;
            vm.SelectedItems = AssociatedObject.SelectedItems.Cast<ProductDataViewModel>();

            if (AssociatedObject.SelectedItems.Count <= 1)
            {
                AssociatedObject.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
                return;
            }
            AssociatedObject.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
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
                var vm = AssociatedObject.DataContext.CastTo<InventoryDataGridViewModel>();

                if (vm.SelectionInfo is null)
                    return;

                if (!isFromSwitch && !vm.SelectionInfo.CanSelect)
                    return;

                var selectionMap = vm.SelectionInfo.Items.ToHashSet();

                AssociatedObject.SelectedItems.Clear();

                foreach (var item in AssociatedObject.Items.Cast<ProductDataViewModel>())
                {
                    if (selectionMap.Contains(item.Item.Id))
                        AssociatedObject.SelectedItems.Add(item);
                }

                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (AssociatedObject.SelectedItem == null)
                        return;

                    AssociatedObject.ScrollIntoView(AssociatedObject.SelectedItem);
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
        public bool IgnoreReorder { get; internal set; }
    }
}
