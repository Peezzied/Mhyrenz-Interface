using System.Windows;
using Mhyrenz_Interface.Shared.Attached;

namespace Mhyrenz_Interface.Features.Inventory.Controls
{
    public class InventoryDataGridColumn
    {
        public static readonly DependencyProperty IgnoreReorderProperty = DependencyProperty.RegisterAttached(
            "IgnoreReorder", typeof(bool), typeof(ValidationHelper), new PropertyMetadata(false));
        public static bool GetIgnoreReorder(DependencyObject obj) => (bool)obj.GetValue(IgnoreReorderProperty);
        public static void SetIgnoreReorder(DependencyObject obj, bool value) => obj.SetValue(IgnoreReorderProperty, value);

        public static readonly DependencyProperty IgnoreVisibilityToggleProperty = DependencyProperty.RegisterAttached(
            "IgnoreVisibilityToggle", typeof(bool), typeof(ValidationHelper), new PropertyMetadata(false));
        public static bool GetIgnoreVisibilityToggle(DependencyObject obj) => (bool)obj.GetValue(IgnoreVisibilityToggleProperty);
        public static void SetIgnoreVisibilityToggle(DependencyObject obj, bool value) => obj.SetValue(IgnoreVisibilityToggleProperty, value);


        public static bool GetPharmaColumn(DependencyObject obj)
        {
            return (bool)obj.GetValue(PharmaColumnProperty);
        }

        public static void SetPharmaColumn(DependencyObject obj, bool value)
        {
            obj.SetValue(PharmaColumnProperty, value);
        }

        public static readonly DependencyProperty PharmaColumnProperty =
            DependencyProperty.RegisterAttached("PharmaColumn", typeof(bool), typeof(InventoryDataGridColumn), new PropertyMetadata(false));


        public static bool GetPlaceOrderBound(DependencyObject obj)
        {
            return (bool)obj.GetValue(PlaceOrderBoundProperty);
        }

        public static void SetPlaceOrderBound(DependencyObject obj, bool value)
        {
            obj.SetValue(PlaceOrderBoundProperty, value);
        }

        public static readonly DependencyProperty PlaceOrderBoundProperty =
            DependencyProperty.RegisterAttached("PlaceOrderBound", typeof(bool), typeof(InventoryDataGridColumn), new PropertyMetadata(false));
    }
}
