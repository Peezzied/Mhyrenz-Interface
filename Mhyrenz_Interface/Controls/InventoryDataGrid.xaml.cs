using Mhyrenz_Interface.ViewModels.Factory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// <summary>
    /// Interaction logic for InventoryDataGrid.xaml
    /// </summary>
    public partial class InventoryDataGrid : UserControl
    {
        public InventoryDataGrid()
        {
            InitializeComponent();
        }

        private void OnUserControlUnload(object sender, EventArgs e)
        {
            IEnumerable source = ((DataGrid)sender).ItemsSource;
            var view = (IEditableCollectionView)CollectionViewSource.GetDefaultView(source);
            view?.CommitEdit();
        }


    }
}
