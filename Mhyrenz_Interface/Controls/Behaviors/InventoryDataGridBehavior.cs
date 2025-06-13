
using Mhyrenz_Interface.ViewModels;
using Microsoft.Xaml.Behaviors;
using System;
using System.Runtime.Remoting.Contexts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Mhyrenz_Interface.Controls.Behaviors
{
    public class InventoryDataGridBehavior : Behavior<DataGrid>
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
            grid.CommitEdit(DataGridEditingUnit.Cell, true);
            grid.CommitEdit(DataGridEditingUnit.Row, true);
        }


        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true; // Cancels the context menu
        }

        private void GetCell(MouseButtonEventArgs e, out DataGridCell dataGrid)
        {
            dataGrid = FindParent<DataGridCell>(e.OriginalSource as DependencyObject);
        }

        private static bool state = false;

        private void OnRightClick(object sender, MouseButtonEventArgs e)
        {
            GetCell(e, out var cell);
            if (cell != null && cell.DataContext is ProductViewModel vm)
                vm.IsCtrlClicked = !vm.IsCtrlClicked;

            state = true;

            ClickHandler(sender, e);
        }

        private void OnLeftClick(object sender, MouseButtonEventArgs e)
        {
            if (state)
            {
                state = false;
                return;
            }

            GetCell(e, out var cell);
            if (cell != null && cell.DataContext is ProductViewModel vm)
                vm.IsCtrlClicked = false;

            ClickHandler(sender, e);
        }

        private void ClickHandler(object sender, MouseButtonEventArgs e)
        {
            GetCell(e, out var cell);
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

            if (e.Row.Item is ProductViewModel vm)
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

                var presenter = FindVisualChild<DataGridCellsPresenter>(row);
                if (presenter == null)
                {
                    dataGrid.UpdateLayout();
                    presenter = FindVisualChild<DataGridCellsPresenter>(row);
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

        private static T FindParent<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T found)
                    return found;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T found)
                    return found;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
