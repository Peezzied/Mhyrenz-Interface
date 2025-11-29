using System.Windows;
using System.Windows.Controls;

namespace Mhyrenz_Interface.Controls
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


    }
}
