using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.ViewModels;

namespace Mhyrenz_Interface.State
{
    public interface ICategoryStore
    {
        event Action Updated;
        ICommand LoadCategoriesCommand { get; }
        Dictionary<Category, Predicate<object>> CategoriesFilter { get; }
        Dictionary<int, Brush> Colors { get; set; }

        Task UpdateCategories();
    }
}