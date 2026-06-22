
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Features.Inventory.Controls;
using Microsoft.Xaml.Behaviors;

namespace Mhyrenz_Interface.Shared.Behaviors
{
    public class DynamicCellControlBehavior : Behavior<DataGrid>
    {
        public static bool GetRightClickBound(DependencyObject obj)
        {
            return (bool)obj.GetValue(RightClickBoundProperty);
        }

        public static void SetRightClickBound(DependencyObject obj, bool value)
        {
            obj.SetValue(RightClickBoundProperty, value);
        }

        public static readonly DependencyProperty RightClickBoundProperty =
            DependencyProperty.RegisterAttached("RightClickBound", typeof(bool), typeof(DynamicCellControlBehavior), new PropertyMetadata(false));

        public static bool GetIsRightClicked(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsRightClickedProperty);
        }

        public static void SetIsRightClicked(DependencyObject obj, bool value)
        {
            obj.SetValue(IsRightClickedProperty, value);
        }

        public static readonly DependencyProperty IsRightClickedProperty =
            DependencyProperty.RegisterAttached("IsRightClicked", typeof(bool), typeof(DynamicCellControlBehavior), new PropertyMetadata(false));


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
            //AssociatedObject.CommitEdit(DataGridEditingUnit.Cell, true);
            //AssociatedObject.CommitEdit(DataGridEditingUnit.Row, true);
        }


        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (AssociatedObject.CurrentColumn != null && !GetRightClickBound(AssociatedObject.CurrentColumn))
                return;

            e.Handled = true;
        }

        private void OnRightClick(object sender, MouseButtonEventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var cell = TreeHelper.TryFindParent<DataGridCell>(
                    (DependencyObject)e.OriginalSource);

                if (cell == null)
                    return;

                if (!cell.IsSelected)
                    return;

                if (!GetRightClickBound(cell.Column))
                    return;

                SetIsRightClicked(AssociatedObject, true);
                cell.Focus();
                AssociatedObject.BeginEdit();
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        private void OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            SetIsRightClicked(AssociatedObject, false);
        }
    }
}
