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
        event Action Loaded;

        void LoadProducts(IEnumerable<Product> products);
        Task InitializeAsync();
        IEnumerable<ProductDataViewModel> AddProduct(ICollection<Product> products);
    }
}