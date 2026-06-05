using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Features.Inventory.ViewModels;

namespace Mhyrenz_Interface.Store
{
    public interface ICategoryStore
    {
        event Action Updated;
        ICommand LoadCategoriesCommand { get; }
        Dictionary<Category, Predicate<ProductDataViewModel>> CategoriesFilter { get; }
        Dictionary<int, Brush> Colors { get; set; }
        Dictionary<int, Category> Categories { get; }

        Task UpdateCategories();
    }
}