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
        private int SelectInto;
        private bool CanSelectInto;
        private bool _overrideCurrentCell;

        protected override void OnAttached()
        {
            AssociatedObject.SelectionChanged += DataGrid_SelectionChanged;
            AssociatedObject.LoadingRow += AssociatedObject_LoadingRow;
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
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                SelectRow(() => AssociatedObject.LoadingRow -= AssociatedObject_LoadingRow);
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        private void InventoryDataGridSelect_SwitchSelectedItem(bool canSelect)
        {
            SelectRow(() => AssociatedObject.DataContext.CastTo<InventoryDataGridViewModel>().SwitchSelectedItem -= InventoryDataGridSelect_SwitchSelectedItem, 
                canSelect: canSelect);
        }

        private void AssociatedObject_LoadingRow(object sender, EventArgs e)
        {
        }

        private async void SelectRow(Action dispose, bool canSelect = true)
        {
            await App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var vm = AssociatedObject.DataContext.CastTo<InventoryDataGridViewModel>();
                if (vm.SwitchSelectItem < 0 && !vm.IsDiff)
                    return;

                AssociatedObject.SelectedIndex = vm.SwitchSelectItem;
                AssociatedObject.ScrollIntoView(AssociatedObject.SelectedItem);
                if (!canSelect)
                    AssociatedObject.SelectedIndex = -1;

                if (!vm.IsDiff)
                    vm.SwitchSelectItem = -1;
                if (vm.IsDiff)
                {
                    dispose();
                }
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);

        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= DataGrid_SelectionChanged;
            AssociatedObject.LoadingRow -= AssociatedObject_LoadingRow;
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            base.OnDetaching();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = (InventoryDataGridViewModel)AssociatedObject.DataContext;

            vm.SelectedItems = AssociatedObject.SelectedItems.Cast<ProductDataViewModel>();

            //Debug.WriteLine($"{vm.SelectedItems.ElementAt(0).Name} SELECTED!");
            vm.RemoveItems = () =>
            {
                var collection = (ListCollectionView)AssociatedObject.ItemsSource;
                var sourceCollection = (ObservableCollection<ProductDataViewModel>)collection.SourceCollection;
                foreach (var item in AssociatedObject.SelectedItems.Cast<ProductDataViewModel>().ToList())
                {
                    sourceCollection.Remove(item);
                }
            };
        }
    }
}
