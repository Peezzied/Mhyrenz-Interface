using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.State
{
    public interface IInventoryStore
    {
        ObservableCollection<ProductDataViewModel> Products { get; }
        ICollectionView ProductsCollectionView { get; set; }
        ILookup<string, ProductDataViewModel> ProductsCollectionViewByCategory { get; set; }
        event EventHandler<InventoryStoreEventArgs> PropertyChanged;
        event EventHandler<InventoryStoreEventArgs> PurchaseEvent;
        event Action PromptSessionEvent;

        Task Register(IEnumerable<Product> transactions);
        void LoadProducts(IEnumerable<Product> products);
    }
}