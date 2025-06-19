using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Mhyrenz_Interface.State
{
    public interface IInventoryStore
    {
        ObservableCollection<ProductDataViewModel> Products { get; }
        ICollectionView ProductsCollectionView { get; set; }
        event EventHandler<InventoryStoreEventArgs> PropertyChanged;
        event EventHandler<InventoryStoreEventArgs> PurchaseEvent;
        void LoadProducts(IEnumerable<Product> products);
    }
}