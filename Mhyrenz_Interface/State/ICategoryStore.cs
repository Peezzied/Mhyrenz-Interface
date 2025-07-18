using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Mhyrenz_Interface.State
{
    public interface CategoryCollection : IDictionary<Category, ICollectionView> { }
    public interface ICategoryStore
    {
        Dictionary<Category, ICollectionView> Categories { get; }
        //ObservableCollection<Category> Categories { get; }
        event Action Updated;
        ICommand LoadCategoriesCommand { get; }
        Dictionary<int, Brush> Colors { get; set; }

        Task UpdateCategories();
    }
}