using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Features.Orders.ViewModels
{
    public class OrderViewModel : TrackedViewModel
    {

        private Order _order;
        public Order Order
        {
            get => _order;
            set
            {
                _order = value;
                _qty = Order.Qty;

                OnPropertyChanged(null);
            }
        }

        private int _qty;
        public int Qty
        {
            get => _qty;
            set
            {
                if (_qty != value)
                {
                    SetTrackedProperty(ref _qty, value, nameof(Qty));
                    OnPropertyChanged(null);
                }
            }
        }

        public decimal RetailPrice => Order.Product.RetailPrice;

        public decimal ListPrice => Order.Product.ListPrice;

        public string Category => Order.Product.Category.Name;

        public string Name => Order.Product.Name;
    }
}
