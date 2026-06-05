using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Features.Inventory.ViewModels;

namespace Mhyrenz_Interface.Store
{
    public interface IInventoryStore : IViewModelStore<int, ProductDataViewModel>
    {
        event EventHandler<InventoryStoreEventArgs> PropertyChanged;
        event EventHandler<InventoryStoreEventArgs> PurchaseEvent;
        event EventHandler<IEnumerable<ProductDataViewModel>> AddProductEvent;
        event EventHandler<IEnumerable<ProductDataViewModel>> RemoveProductEvent;
        event Action Loaded;

        Task Register(IEnumerable<Product> transactions);
        void LoadProducts(IEnumerable<Product> products);
        Task InitializeAsync();
        void RemoveProduct(IEnumerable<ProductDataViewModel> product);
        IEnumerable<ProductDataViewModel> AddProduct(ICollection<Product> products);
        void PurchaseProduct(int productId, int amount);
    }
}