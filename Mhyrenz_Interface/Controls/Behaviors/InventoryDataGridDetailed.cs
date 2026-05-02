using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Controls.Attached;
using Mhyrenz_Interface.Controls.Columns;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.ViewModels;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Xaml.Behaviors;

namespace Mhyrenz_Interface.Controls.Behaviors
{
    public class InventoryDataGridDetailed : Behavior<DataGrid>
    {

        public static readonly DependencyProperty TabOwnerProperty =
            DependencyProperty.Register(
                "TabOwner",
                typeof(InventoryTabItem),
                typeof(InventoryDataGridDetailed),
                new PropertyMetadata(null, OnTabOwnerChanged));

        public static event Action<InventoryTabItem> TabChanged;

        public InventoryTabItem TabOwner
        {
            get => (InventoryTabItem)GetValue(TabOwnerProperty);
            set => SetValue(TabOwnerProperty, value);
        }

        private static void OnTabOwnerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (InventoryDataGridDetailed)d;

            if (!(e.NewValue is InventoryTabItem tab) || behavior.AssociatedObject == null)
                return;

            //behavior.InventoryDataGridDetailed_TabChanged(tab);
            TabChanged?.Invoke(tab);
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
            TabChanged -= InventoryDataGridDetailed_TabChanged;
            TabChanged -= InventoryDataGridSelect;

            AssociatedObject.SelectionChanged -= DataGrid_SelectionChanged;
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;

            var vm = AssociatedObject.DataContext.CastTo<InventoryDataGridViewModel>();
            vm.SwitchSelectedItem -= InventoryDataGridSelect_SwitchSelectedItem;
            vm.CommitEdits -= InventoryDataGridDetailed_CommitEdits;
        }

        private void InventoryDataGridDetailed_TabChanged(InventoryTabItem tab)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var vm = AssociatedObject.DataContext.CastTo<InventoryDataGridViewModel>();

                if (vm.ColumnExtras?.Any() ?? false)
                {
                    foreach (var item in vm.ColumnExtras)
                    {
                        var name = item.Value.Name;
                        var type = item.Value.Type;
                        var field = item.Value.Field;

                        switch ((ColumnType)Enum.Parse(typeof(ColumnType), type))
                        {
                            case ColumnType.Number:
                                break;

                            case ColumnType.Text:
                                var column = new TextColumn
                                {
                                    Header = name,
                                    ValuePath = $"{nameof(ProductDataViewModel.Extras)}[{field}].Value",
                                };

                                AssociatedObject.Columns.Add(column);
                                InventoryDataGridColumn.SetColumnPath(column, field);
                                break;
                        }
                    }
                }

                tab.LoadColumns(AssociatedObject.Columns.Select(x => new ColumnInfo
                {
                    Header = x.Header?.ToString(),
                    DisplayIndex = x.DisplayIndex,
                    IgnoreReorder = InventoryDataGridColumn.GetIgnoreReorder(x),
                    IgnoreVisibilityToggle = InventoryDataGridColumn.GetIgnoreVisibilityToggle(x),
                }));

                foreach (var col in AssociatedObject.Columns)
                {
                    if (col.Header == null) continue;

                    var columnPath = InventoryDataGridColumn.GetColumnPath(col);

                    BindingOperations.ClearBinding(col, DataGridColumn.VisibilityProperty);
                    BindingOperations.ClearBinding(col, DataGridColumn.DisplayIndexProperty);

                    BindingOperations.SetBinding(col, DataGridColumn.VisibilityProperty, new Binding
                    {
                        Source = vm,
                        Path = new PropertyPath($"{nameof(InventoryDataGridViewModel.ColumnsSettings)}[{columnPath ?? col.Header}].{nameof(ColumnSettingViewModel.IsVisible)}"),
                        Converter = new BooleanToVisibilityConverter(),
                    });

                    BindingOperations.SetBinding(col, DataGridColumn.DisplayIndexProperty, new Binding
                    {
                        Source = vm,
                        Path = new PropertyPath($"{nameof(InventoryDataGridViewModel.ColumnsSettings)}[{columnPath ?? col.Header}].{nameof(ColumnSettingViewModel.DisplayIndex)}"),
                        Mode = BindingMode.TwoWay
                    });
                }
            }));
        }

        private void InventoryDataGridSelect(InventoryTabItem item)
        {
            item.ContentViewModel.SwitchSelectedItem += InventoryDataGridSelect_SwitchSelectedItem;
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
