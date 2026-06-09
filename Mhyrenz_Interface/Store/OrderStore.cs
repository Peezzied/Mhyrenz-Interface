using System;
using System.Linq;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Core.Collection;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Features.Orders.ViewModels;
using Mhyrenz_Interface.Shared.Behaviors;
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

        public async void AddItem(Order order, int productId)
        {
            if (order == null)
            {
                if (!Store.TryGetValue(productId, out var item))
                    return;

                await item.RequestFlash(DataGridFlashBehavior.OperationType.Remove);
                Store.Remove(productId);
                return;
            }

            if (Store.TryGetValue(productId, out var existing))
            {
                existing.Order = order;
                await existing.RequestFlash(DataGridFlashBehavior.OperationType.Update);
                return;
            }

            var vm = _createViewModel(order);
            var categoryId = order.Product.CategoryId;

            vm.CategoryColor = _categoryStore.Colors[categoryId];
            vm.CategoryName = _categoryStore.Categories[categoryId].Name;

            Store.Add(vm);

            App.Current.BeginInvoke(new Action(() => vm.RequestFlash(DataGridFlashBehavior.OperationType.New)));
        }

        public async Task InitializeAsync()
        {
            var orders = (await _orderService.GetOrders())
                .Select(order =>
                {
                    var vm = _createViewModel(order);

                    vm.CategoryColor = _categoryStore.Colors[order.Product.CategoryId];
                    vm.CategoryName = _categoryStore.Categories[order.Product.CategoryId].Name;

                    return vm;
                })
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
