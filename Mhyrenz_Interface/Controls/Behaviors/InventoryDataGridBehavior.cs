
using HandyControl.Tools.Extension;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.ViewModels;
using Microsoft.Xaml.Behaviors;
using System;
using System.Drawing;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Mhyrenz_Interface.Controls.Behaviors
{
    public partial class InventoryDataGridBehavior : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            AssociatedObject.CellEditEnding += OnCellEditEnding;
            AssociatedObject.CurrentCellChanged += OnCellChanged;
            AssociatedObject.ContextMenuOpening += OnContextMenuOpening;
            AssociatedObject.PreviewMouseRightButtonDown += OnRightClick;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.CellEditEnding -= OnCellEditEnding;
            AssociatedObject.CurrentCellChanged -= OnCellChanged;
            AssociatedObject.ContextMenuOpening -= OnContextMenuOpening;
            AssociatedObject.PreviewMouseRightButtonDown -= OnRightClick;
        }
        private void OnCellChanged(object sender, EventArgs e)
        {
            var grid = (DataGrid)sender;
            App.Current.Dispatcher.Invoke(() =>
            {
                if (_state)
                {
                    _state = false;
                    return;
                }
                grid.CommitEdit(DataGridEditingUnit.Cell, true);
                grid.CommitEdit(DataGridEditingUnit.Row, true);
            }, System.Windows.Threading.DispatcherPriority.Input);
        }


        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private bool _state = false;

        private void OnRightClick(object sender, MouseButtonEventArgs e)
        {
            var cell = TreeHelper.TryFindParent<DataGridCell>(e.OriginalSource as DependencyObject);
            

            if (cell != null && cell.DataContext is ProductDataViewModel vm)
            {
                sender.CastTo<DataGrid>().SelectedItem = vm;
                vm.IsCtrlClicked = true;
            }

            _state = true;

            ClickHandler(sender, e);
            
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var inputBox = TreeHelper.FindChild<NumericUpDown>(cell);

                    if (cell != null && cell.DataContext is ProductDataViewModel && inputBox != null)
                    {
                        inputBox.Focus();   
                        Keyboard.Focus(inputBox);

                        AssociatedObject.DataContext.CastTo<InventoryDataGridViewModel>().GetCell = () => cell;
                    }

                }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        private void OnLeftClick(object sender, MouseButtonEventArgs e)
        {
            if (_state)
            {
                _state = false;
                return;
            }

            var cell = TreeHelper.TryFindParent<DataGridCell>(e.OriginalSource as DependencyObject);
            if (cell != null && cell.DataContext is ProductDataViewModel vm)
                vm.IsCtrlClicked = false;

            ClickHandler(sender, e);
        }

        private void ClickHandler(object sender, MouseButtonEventArgs e)
        {
            var cell = TreeHelper.TryFindParent<DataGridCell>(e.OriginalSource as DependencyObject);
            if (cell == null) return;

            var dataGrid = AssociatedObject;
            var column = cell.Column;

            // Strongly check header
            if (column?.Header?.ToString() != "Purchase") return;

            var rowData = cell.DataContext;
            if (rowData == null) return;

            BeginEditOnCell(dataGrid, rowData, column);
        }

        private void OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

            if (e.Row.Item is ProductDataViewModel vm)
            {
                vm.IsCtrlClicked = false;
            }
        }

        private static void BeginEditOnCell(DataGrid dataGrid, object rowData, DataGridColumn column)
        {
            dataGrid.Dispatcher.InvokeAsync(() =>
            {
                dataGrid.ScrollIntoView(rowData, column);
                dataGrid.UpdateLayout();

                var row = dataGrid.ItemContainerGenerator.ContainerFromItem(rowData) as DataGridRow;
                if (row == null) return;

                var presenter = TreeHelper.FindChild<DataGridCellsPresenter>(row);
                if (presenter == null)
                {
                    dataGrid.UpdateLayout();
                    presenter = TreeHelper.FindChild<DataGridCellsPresenter>(row);
                    if (presenter == null) return;
                }

                var columnIndex = dataGrid.Columns.IndexOf(column);
                var cell = presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex) as DataGridCell;
                if (cell == null) return;

                // Proper way to set current cell in SelectionUnit=Cell mode
                dataGrid.CurrentCell = new DataGridCellInfo(cell);

                // Ensure focus is on the cell
                cell.Focus();

                // Begin editing the cell
                dataGrid.BeginEdit();
            });
        }
    }
}
