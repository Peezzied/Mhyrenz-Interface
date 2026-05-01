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
        public InventoryTabItem TabOwner
        {
            get { return (InventoryTabItem)GetValue(TabOwnerProperty); }
            set { SetValue(TabOwnerProperty, value); }
        }

        public static readonly DependencyProperty TabOwnerProperty =
            DependencyProperty.Register("TabOwner", typeof(InventoryTabItem), typeof(InventoryDataGridDetailed), new PropertyMetadata(null, OnTabOwnerChanged));

        public static event Action<InventoryTabItem> TabChanged;

        private static void OnTabOwnerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (InventoryDataGridDetailed)d;
            var tab = e.NewValue as InventoryTabItem;

            if (tab == null || behavior.AssociatedObject == null)
                return;

            behavior.InventoryDataGridDetailed_TabChanged(tab);
        }

        public InventoryDataGridDetailed()
        {
            TabChanged += InventoryDataGridDetailed_TabChanged;
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
                                //AssociatedObject.Columns.Add(new NumberColumn
                                //{
                                //    ValuePath = 
                                //});
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

                            default:
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
                    if (col.Header != null)
                    {
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
                }
            }));
        }

        protected override void OnAttached()
        {
            AssociatedObject.SelectionChanged += DataGrid_SelectionChanged;
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;

            base.OnAttached();
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            var vm = AssociatedObject.DataContext.CastTo<InventoryDataGridViewModel>();

            vm.SwitchSelectedItem -= InventoryDataGridSelect_SwitchSelectedItem;
            vm.CommitEdits -= InventoryDataGridDetailed_CommitEdits;

        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.DataContext.CastTo<InventoryDataGridViewModel>().SwitchSelectedItem += InventoryDataGridSelect_SwitchSelectedItem;
            AssociatedObject.DataContext.CastTo<InventoryDataGridViewModel>().CommitEdits += InventoryDataGridDetailed_CommitEdits;

            var vm = AssociatedObject.DataContext.CastTo<InventoryDataGridViewModel>();

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                SelectRow();
            }), DispatcherPriority.Background);
        }

        private void InventoryDataGridDetailed_CommitEdits()
        {
            AssociatedObject.CancelEdit(DataGridEditingUnit.Cell);
            AssociatedObject.CancelEdit(DataGridEditingUnit.Row);
        }

        private void InventoryDataGridSelect_SwitchSelectedItem()
        {
            SelectRow(isFromSwitch: true);
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
                    if (AssociatedObject.SelectedItem is null)
                    {
                        AssociatedObject.SelectedIndex = vm.SelectionInfo.Index;
                        AssociatedObject.ScrollIntoView(AssociatedObject.SelectedItem);
                        AssociatedObject.SelectedIndex = -1;
                        return;
                    }
                    AssociatedObject.ScrollIntoView(AssociatedObject.SelectedItem);
                }), DispatcherPriority.ContextIdle);

                vm.SelectionInfo.CanSelect = false;

                //if (!canSelect)
                //    AssociatedObject.SelectedIndex = -1;
            }), DispatcherPriority.ContextIdle);

        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = (InventoryDataGridViewModel)AssociatedObject.DataContext;

            vm.SelectedItems = AssociatedObject.SelectedItems.Cast<ProductDataViewModel>();

            //Debug.WriteLine($"{vm.SelectedItems.ElementAt(0).Name} SELECTED!");
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
