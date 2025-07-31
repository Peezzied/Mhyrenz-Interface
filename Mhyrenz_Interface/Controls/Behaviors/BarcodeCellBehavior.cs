using HandyControl.Tools.Extension;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.ViewModels;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mhyrenz_Interface.Controls.Behaviors
{
    public class BarcodeCellBehavior: Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        private void BarcodeCellBehavior_BarcodeReceived()
        {
            var vm = AssociatedObject.DataContext as BaseViewModel;

            var dataGrid = TreeHelper.TryFindParent<DataGrid>(AssociatedObject);

            dataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            dataGrid.CommitEdit(DataGridEditingUnit.Row, true); 

            vm.Dispose();
        }

        private void AssociatedObject_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
                }

        private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            AssociatedObject.DataContext.CastTo<IBarcodeBound>().BarcodeReceived += BarcodeCellBehavior_BarcodeReceived;
            var vm = AssociatedObject.DataContext.CastTo<IBarcodeBound>();

            vm.Load();
        }
    }
}
