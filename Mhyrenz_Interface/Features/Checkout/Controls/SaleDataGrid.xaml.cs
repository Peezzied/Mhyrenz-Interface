using System.Linq;
using System.Windows.Controls;
using Mhyrenz_Interface.Features.Checkout.ViewModels;

namespace Mhyrenz_Interface.Features.Checkout.Controls
{
    /// <summary>
    /// Interaction logic for SaleDataGrid.xaml
    /// </summary>
    public partial class SaleDataGrid : UserControl
    {
        public SaleDataGrid()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = (SaleTabItem)DataGrid.DataContext;

            if (vm != null)
                vm.SelectedItems = DataGrid.SelectedItems.OfType<TransactionDataViewModel>();
        }
    }
}
