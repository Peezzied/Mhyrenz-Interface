using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.CategoryService;
using Mhyrenz_Interface.ViewModels;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Mhyrenz_Interface.State
{
    public class CategoryStore : ICategoryStore
    {
        private readonly IInventoryStore _inventoryStore;
        private readonly ICategoryService _categoryService;

        //public ObservableCollection<Category> Categories { get; private set; } = new ObservableCollection<Category>();

        public event Action Updated;

        private Dictionary<Category, ICollectionView> _categories = new Dictionary<Category, ICollectionView>();
        public Dictionary<Category, ICollectionView> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnChange();
            }
        }

        public Dictionary<int, Brush> Colors { get; set; } = new Dictionary<int, Brush>();

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
            Categories.Clear();

            var result = await _categoryService.GetAllCategories();

            foreach (var item in result)
            {
                Categories[item] = new ListCollectionView(_inventoryStore.Products);
                Categories[item].Filter += obj => obj is ProductDataViewModel vm
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
