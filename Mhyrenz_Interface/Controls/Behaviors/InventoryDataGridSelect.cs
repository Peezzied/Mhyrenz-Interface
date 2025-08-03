using HandyControl.Tools.Extension;
using Mhyrenz_Interface.ViewModels;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Mhyrenz_Interface.Controls.Behaviors
{
    public class InventoryDataGridSelect : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            AssociatedObject.SelectionChanged += DataGrid_SelectionChanged;
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
            base.OnAttached();
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.DataContext.CastTo<InventoryDataGridViewModel>().SwitchSelectedItem -= InventoryDataGridSelect_SwitchSelectedItem;
            //AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.DataContext.CastTo<InventoryDataGridViewModel>().SwitchSelectedItem += InventoryDataGridSelect_SwitchSelectedItem;
            App.Current.Dispatcher.BeginInvoke(new Action(() => SelectRow()), System.Windows.Threading.DispatcherPriority.Background);
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

                var selectionMap = vm.SelectionInfo.Items.Select(i => i.Item.Id).ToHashSet();
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
                }), System.Windows.Threading.DispatcherPriority.ContextIdle);

                vm.SelectionInfo.CanSelect = false;

                //if (!canSelect)
                //    AssociatedObject.SelectedIndex = -1;
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);

        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = (InventoryDataGridViewModel)AssociatedObject.DataContext;

            vm.SelectedItems = AssociatedObject.SelectedItems.Cast<ProductDataViewModel>();

            //Debug.WriteLine($"{vm.SelectedItems.ElementAt(0).Name} SELECTED!");
        }
    }
}
