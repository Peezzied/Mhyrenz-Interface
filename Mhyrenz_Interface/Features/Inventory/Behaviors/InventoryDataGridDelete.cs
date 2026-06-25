using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Microsoft.Xaml.Behaviors;

namespace Mhyrenz_Interface.Features.Inventory.Behaviors
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
                e.Handled = true;
                List<ProductDataViewModel> selectedItems = AssociatedObject.SelectedItems.Cast<ProductDataViewModel>().ToList();

                Command.Execute(selectedItems);
            }
        }

    }
}
