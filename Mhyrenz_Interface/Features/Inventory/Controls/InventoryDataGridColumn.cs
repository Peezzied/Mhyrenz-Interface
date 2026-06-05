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

        public static readonly DependencyProperty IgnoreProperty = DependencyProperty.RegisterAttached(
            "Ignore", typeof(bool), typeof(ValidationHelper), new PropertyMetadata(false));
        public static bool GetIgnore(DependencyObject obj) => (bool)obj.GetValue(IgnoreProperty);
        public static void SetIgnore(DependencyObject obj, bool value) => obj.SetValue(IgnoreProperty, value);


        public static readonly DependencyProperty ColumnPathProperty = DependencyProperty.RegisterAttached(
            "ColumnPath", typeof(string), typeof(ValidationHelper), new PropertyMetadata(null));
        public static string GetColumnPath(DependencyObject obj) => (string)obj.GetValue(ColumnPathProperty);
        public static void SetColumnPath(DependencyObject obj, string value) => obj.SetValue(ColumnPathProperty, value + "Column");

    }
}
