using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.CategoryService;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Mhyrenz_Interface.Commands
{
    public class LoadCategoriesCommand : BaseAsyncCommand
    {
        private readonly ICategoryService _categoryService;
        private readonly IInventoryStore _inventoryStore;
        private readonly ICategoryStore _categoryStore;
        public LoadCategoriesCommand(ICategoryStore categoryStore, ICategoryService categoryService, IInventoryStore inventoryStore)
        {
            _categoryStore = categoryStore;
            _categoryService = categoryService;
            _inventoryStore = inventoryStore;
        }

        public override async Task ExecuteAsync(object parameter)
        {
            var result = await _categoryService.GetAllCategories();

            foreach (var item in result)
            {
                _categoryStore.Categories[item] = new ListCollectionView(_inventoryStore.Products)
                {
                    Filter = (obj) => obj is ProductDataViewModel vm
                        && vm.CategoryId == item.Id
                };
            }

        }
    }
}
