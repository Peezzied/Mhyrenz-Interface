using System;
using System.Drawing;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Shared.Behaviors;
using Brush = System.Windows.Media.Brush;

namespace Mhyrenz_Interface.Features.Orders.ViewModels
{
    public class OrderDataViewModel : TrackedViewModel, IFlashRequestable
    {
        public OrderDataViewModel(Order order)
        {
            Order = order;
        }

        public event EventHandler<RowFlashRequestedEventArgs> FlashRequested;

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

        public string CategoryName { get; set; }

        public Brush CategoryColor { get; set; }

        public string Name => Order.Product.Name;

        public Task RequestFlash(DataGridFlashBehavior.OperationType type)
        {
            var args = new RowFlashRequestedEventArgs(type);
            FlashRequested?.Invoke(this, args);

            return args.Completion.Task;
        }
    }
}
