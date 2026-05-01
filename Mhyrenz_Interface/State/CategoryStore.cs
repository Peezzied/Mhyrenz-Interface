using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.CategoryService;
using Mhyrenz_Interface.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Mhyrenz_Interface.State
{
    public class CategoryStore : ICategoryStore
    {
        private readonly IInventoryStore _inventoryStore;
        private readonly ICategoryService _categoryService;

        //public ObservableCollection<Category> Categories { get; private set; } = new ObservableCollection<Category>();

        public event Action Updated;

        public Dictionary<int, Brush> Colors { get; set; } = new Dictionary<int, Brush>();
        public Dictionary<Category, Predicate<object>> CategoriesFilter { get; private set; } = new Dictionary<Category, Predicate<object>>();

        private void OnChange()
        {
            Updated?.Invoke();
        }

        public ICommand LoadCategoriesCommand { get; }

        public CategoryStore(ICategoryService categoryService, IInventoryStore inventoryStore)
        {
            _inventoryStore = inventoryStore;
            _categoryService = categoryService;

            //_inventoryStore.AddProductEvent += OnAddProduct;

            LoadCategoriesCommand = new LoadCategoriesCommand(this, _categoryService, _inventoryStore);
        }

        //private void OnAddProduct(object sender, ProductDataViewModel vm)
        //{
        //    UpdateCategories();
        //}

        public async Task UpdateCategories()
        {
            CategoriesFilter.Clear();

            var result = await _categoryService.GetAllCategories();

            foreach (var item in result)
            {
                CategoriesFilter[item] = obj => obj is ProductDataViewModel vm
                    && vm.CategoryId == item.Id;
            }
        }

        public static async Task LoadCategoryStore(IServiceProvider serviceProvider)
        {
            var categoryStore = serviceProvider.GetRequiredService<ICategoryStore>();
            await categoryStore.UpdateCategories();
        }
    }
}
