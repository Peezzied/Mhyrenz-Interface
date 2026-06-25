using System;
using System.Linq;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core.Collection;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Features.Orders.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Mhyrenz_Interface.Store
{
    public class OrderStore : IOrderStore
    {
        private readonly CreateViewModel<OrderDataViewModel> _createViewModel;
        private readonly IOrderService _orderService;
        private readonly ICategoryStore _categoryStore;

        public SourceCollection<int, OrderDataViewModel> Store { get; }
            = new SourceCollection<int, OrderDataViewModel>(x => x.Order.ProductId);

        public OrderStore(CreateViewModel<OrderDataViewModel> createViewModel, ICategoryStore categoryStore, IOrderService orderService)
        {
            _createViewModel = createViewModel;
            _orderService = orderService;
            _categoryStore = categoryStore;
        }

        public OrderDataViewModel AddItem(Order order)
        {
            var vm = _createViewModel(order);
            Store.Add(vm);

            return vm;
        }

        public async Task InitializeAsync()
        {
            var orders = (await _orderService.GetOrders())
                .Select(order => _createViewModel(order))
                .ToList();

            Store.AddRange(orders);
        }

        public static async Task LoadOrderStore(IServiceProvider sp)
        {
            var transactionStore = sp.GetRequiredService<IOrderStore>();
            await transactionStore.InitializeAsync();
        }
    }
}
