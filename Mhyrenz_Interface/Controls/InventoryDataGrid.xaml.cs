using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
