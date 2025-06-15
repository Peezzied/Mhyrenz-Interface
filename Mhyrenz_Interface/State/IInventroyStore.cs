using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mhyrenz_Interface.State
{
    public interface IInventoryStore
    {
        ObservableCollection<ProductDataViewModel> Products { get; }
        event EventHandler<InventoryStoreEventArgs> PropertyChanged;
        event EventHandler<InventoryStoreEventArgs> PurchaseEvent;
        void LoadProducts(IEnumerable<Product> products);
    }
}