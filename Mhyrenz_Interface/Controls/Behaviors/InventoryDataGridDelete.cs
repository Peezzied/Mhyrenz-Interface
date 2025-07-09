using Mhyrenz_Interface.ViewModels;
using Microsoft.Xaml.Behaviors;
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
    public class InventoryDataGridDelete : Behavior<DataGrid>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(InventoryDataGridDelete));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            CommandManager.AddPreviewExecutedHandler(AssociatedObject, OnPreviewExecuted);
        }

        protected override void OnDetaching()
        {
            CommandManager.RemovePreviewExecutedHandler(AssociatedObject, OnPreviewExecuted);
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
