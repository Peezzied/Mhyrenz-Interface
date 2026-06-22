using System.Windows;
using System.Windows.Controls;
using HandyControl.Controls;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Features.Inventory.Views;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Mhyrenz_Interface.Shared.Columns
{
    public static class ColumnHelper
    {
        public static bool IsEditable(this DataGridCell cell, Product product)
        {
            var inventoryView = TreeHelper.TryFindParent<InventoryView>(cell);

            if (inventoryView.DataContext is InventoryViewModel vm && vm.ProductsInCheckout.Contains(product.Id))
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
