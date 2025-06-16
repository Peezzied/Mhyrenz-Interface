using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Mhyrenz_Interface.State
{
    public interface ICategoryStore
    {
        ObservableCollection<Category> Categories { get; }
        ICommand LoadCategoriesCommand { get; }
    }
}