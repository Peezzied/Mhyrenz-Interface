using Mhyrenz_Interface.Core.Collection;
using Mhyrenz_Interface.Features.Orders.ViewModels;

namespace Mhyrenz_Interface.Store
{
    public class OrderStore : IOrderStore
    {
        public SourceCollection<int, OrderViewModel> Store { get; }
            = new SourceCollection<int, OrderViewModel>(x => x.Order.ProductId);
    }
}
