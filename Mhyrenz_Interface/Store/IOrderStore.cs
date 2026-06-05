using Mhyrenz_Interface.Features.Orders.ViewModels;

namespace Mhyrenz_Interface.Store
{
    public interface IOrderStore : IViewModelStore<int, OrderViewModel>
    {
    }
}