using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Features.Inventory.Views;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Mhyrenz_Interface.Shared.Columns
{
    public static class ColumnHelper
    {
        public static bool GetMonitorSale(DependencyObject obj)
        {
            return (bool)obj.GetValue(MonitorSaleProperty);
        }

        public static void SetMonitorSale(DependencyObject obj, bool value)
        {
            obj.SetValue(MonitorSaleProperty, value);
        }

        public static readonly DependencyProperty MonitorSaleProperty =
            DependencyProperty.RegisterAttached("MonitorSale", typeof(bool), typeof(ColumnHelper), new PropertyMetadata(false));


        public static bool IsEditable(this DataGridCell cell, object dataItem)
        {
            if (!GetMonitorSale(cell.Column))
                return true;

            var inventoryView = TreeHelper.TryFindParent<InventoryView>(cell);

            if (inventoryView.DataContext is InventoryViewModel vm && vm.ProductsInActiveSales.Contains(((ProductDataViewModel)dataItem).Item.Id))
            {
                MessageBox.Show(
                    "This product cannot be modified or deleted because it is currently part of an active sale.\n\n" +
                    "Please complete or remove the item from the sale before continuing.",
                    "Action Not Allowed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            return true;
        }
    }
}
