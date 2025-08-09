using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
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
        (int Index, IEnumerable<ProductDataViewModel> Products) LastProductChanged { get; set;  }

        event EventHandler<InventoryStoreEventArgs> PropertyChanged;
        event EventHandler<InventoryStoreEventArgs> PurchaseEvent;
        event EventHandler<IEnumerable<ProductDataViewModel>> AddProductEvent;
        event EventHandler<IEnumerable<ProductDataViewModel>> RemoveProductEvent;
        event Action Loaded;

        Task Register(IEnumerable<Product> transactions);
        void LoadProducts(IEnumerable<Product> products);
        Task InitializeAsync();
        void RemoveProduct(IEnumerable<ProductDataViewModel> product);
        IEnumerable<ProductDataViewModel> AddProduct(IEnumerable<Product> products);
        ProductDataViewModel GetProductByIndex(int index);
        ProductDataViewModel GetProductByBarcode(string obj);
        void PurchaseProduct(ProductDataViewModel viewModel, TargetChangedEventArgs args, object oldValue, object newValue, PurchaseProductCommand purchaseProductCommand, PropertyChangeTracker<ProductDataViewModel> tracker = null);
        PropertyChangeTracker<ProductDataViewModel> GetTrackerByProduct(ProductDataViewModel product);
        ProductDataViewModel GetProductById(int id);
    }
}