using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mhyrenz_Interface.State
{
    public interface IInventroyStore
    {
        ObservableCollection<ProductViewModel> Products { get; }
        void LoadProducts(IEnumerable<Product> products);
    }
}