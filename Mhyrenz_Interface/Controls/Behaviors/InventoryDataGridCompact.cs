using HandyControl.Tools.Extension;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.ViewModels;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Mhyrenz_Interface.Controls.Behaviors
{
    public class InventoryDataGridCompact: Behavior<DataGrid>
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
