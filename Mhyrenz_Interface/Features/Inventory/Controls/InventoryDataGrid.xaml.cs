using System.Windows;
using System.Windows.Controls;
using Mhyrenz_Interface.Features.Inventory.ViewModels;

namespace Mhyrenz_Interface.Features.Inventory.Controls
{
    public enum InventoryDataGridLayout
    {
        Compacted, Detailed
    }

    /// <summary>
    /// Interaction logic for InventoryDataGrid.xaml
    /// </summary>
    public partial class InventoryDataGrid : UserControl
    {
        public InventoryDataGrid()
        {
            InitializeComponent();
        }

        public InventoryDataGridLayout Layout
        {
            get { return (InventoryDataGridLayout)GetValue(LayoutProperty); }
            set { SetValue(LayoutProperty, value); }
        }

        public static readonly DependencyProperty LayoutProperty =
            DependencyProperty.Register("Layout", typeof(InventoryDataGridLayout), typeof(InventoryDataGrid), new PropertyMetadata(InventoryDataGridLayout.Compacted));


        public object TabOwner
        {
            get { return (InventoryTabItem)GetValue(TabOwnerProperty); }
            set { SetValue(TabOwnerProperty, value); }
        }

        public static readonly DependencyProperty TabOwnerProperty =
            DependencyProperty.Register("TabOwner", typeof(InventoryTabItem), typeof(InventoryDataGrid), new PropertyMetadata(null));

    }
}
