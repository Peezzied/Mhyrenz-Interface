using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.CategoryService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mhyrenz_Interface.State
{
    public class CategoryStore : ICategoryStore
    {
        private readonly IInventoryStore _inventoryStore;
        public ObservableCollection<Category> Categories { get; private set; } = new ObservableCollection<Category>();
        public ICommand LoadCategoriesCommand { get; }

        public CategoryStore(ICategoryService categoryService, IInventoryStore inventoryStore)
        {
            _inventoryStore = inventoryStore;

            _inventoryStore.PurchaseEvent += OnPurchase;

            LoadCategoriesCommand = new LoadCategoriesCommand(this, categoryService);
        }

        private void OnPurchase(object sender, InventoryStoreEventArgs e)
        {
            UpdateCategories(this);
        }

        private void UpdateCategories(ICategoryStore categoryStore)
        {
            Categories.Clear();
            categoryStore.LoadCategoriesCommand.Execute(null);
        }

        public static CategoryStore LoadCategoryStore(ICategoryService categoryService, IInventoryStore inventoryStore)
        {
            var categoryStore = new CategoryStore(categoryService, inventoryStore);

            categoryStore.UpdateCategories(categoryStore);

            return categoryStore;
        }
    }
}
