namespace Mhyrenz_Interface.ViewModels
{
    public class SaleViewModel: BaseViewModel
    {
        public SaleViewModel()
        {
            
        }

        public int Qty { get; set; }

        public string Name { get; set; }

        public decimal RetailPrice { get; set; }

        public decimal TotalPrice => RetailPrice * Qty;

    }
}