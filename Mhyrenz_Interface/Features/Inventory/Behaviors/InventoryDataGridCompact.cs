using System;
using System.Windows.Controls;
using HandyControl.Tools.Extension;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Microsoft.Xaml.Behaviors;

namespace Mhyrenz_Interface.Features.Inventory.Behaviors
{
    public class InventoryDataGridCompact : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            App.Current.BeginInvoke(new Action(() =>
            {
                AssociatedObject.DataContext.CastTo<InventoryDataGridViewModel>().CommitEdits += InventoryDataGridCompact_CommitEdits;
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        private void InventoryDataGridCompact_CommitEdits()
        {
            AssociatedObject.DataContext.CastTo<InventoryDataGridViewModel>().CommitEdits -= InventoryDataGridCompact_CommitEdits;
            AssociatedObject.CancelEdit(DataGridEditingUnit.Cell);
            AssociatedObject.CancelEdit(DataGridEditingUnit.Row);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
        }
    }
}
