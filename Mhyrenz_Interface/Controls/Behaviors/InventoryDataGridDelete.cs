using Mhyrenz_Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mhyrenz_Interface.Controls.Behaviors
{
    public partial class InventoryDataGridBehavior
    {
        public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(InventoryDataGridBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        partial void OnAttachedExtended()
        {
            base.OnAttached();
            CommandManager.AddPreviewExecutedHandler(AssociatedObject, OnPreviewExecuted);
        }

        partial void OnDetachingExtended()
        {
            CommandManager.RemovePreviewExecutedHandler(AssociatedObject, OnPreviewExecuted);
            base.OnDetaching();
        }

        private void OnPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command.Equals(DataGrid.DeleteCommand) && Command?.CanExecute(null) == true)
            {
                List<ProductDataViewModel> selectedItems = AssociatedObject.SelectedItems.Cast<ProductDataViewModel>().ToList();

                Command.Execute(new InventoryDataGridVmDTO 
                { 
                    ProductData = selectedItems,
                    DeleteHandle = () =>
                    {
                        e.Handled = true;
                    }
                });
            }
        }

    }
}
