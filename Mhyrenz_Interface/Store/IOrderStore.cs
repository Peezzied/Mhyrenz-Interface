using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Features.Orders.ViewModels;

namespace Mhyrenz_Interface.Store
{
    public interface IOrderStore : IViewModelStore<int, OrderDataViewModel>
    {
        void AddItem(Order order, int productId);
        Task InitializeAsync();
    }
}