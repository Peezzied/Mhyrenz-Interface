using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.ViewModels;
using ObservableCollections;
using static Mhyrenz_Interface.State.InventoryStore;

namespace Mhyrenz_Interface.State
{
    public interface IInventoryStore: IViewModelStore<int, ProductDataViewModel>
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
    }
}