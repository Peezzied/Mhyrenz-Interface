using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IInventoryStore _inventoryStore;

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public InventoryDataGridDelete()
        {
            _inventoryStore = App.ServiceProvider.GetRequiredService<IInventoryStore>();
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

                Command.Execute(new InventoryDataGridVmDTO 
                { 
                    ProductData = selectedItems,
                    RemoveItemsHandler = () =>
                    {
                        _inventoryStore.RemoveProduct(selectedItems);
                    }
                });
            }
        }

    }
}
