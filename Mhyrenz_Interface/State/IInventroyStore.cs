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
        (int Index, ProductDataViewModel Product) LastProductChanged { get; set;  }

        event EventHandler<InventoryStoreEventArgs> PropertyChanged;
        event EventHandler<InventoryStoreEventArgs> PurchaseEvent;
        event Action PromptSessionEvent;
        event EventHandler<ProductDataViewModel> AddProductEvent;
        event Action Loaded;

        Task Register(IEnumerable<Product> transactions);
        void LoadProducts(IEnumerable<Product> products);
        ProductDataViewModel AddProduct(Product product);
        Task InitializeAsync();
        void RemoveProduct(IEnumerable<ProductDataViewModel> product);
        void RemoveProduct(ProductDataViewModel product);
        void AddProduct(IEnumerable<Product> products);
        ProductDataViewModel GetProductByIndex(int index);
    }
}