using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Services.CategoryService;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Commands
{
    public class LoadCategoriesCommand : BaseAsyncCommand
    {
        private readonly ICategoryService _categoryService; 
        private readonly ICategoryStore _categoryStore;
        public LoadCategoriesCommand(ICategoryStore categoryStore, ICategoryService categoryService)
        {
            _categoryStore = categoryStore;
            _categoryService = categoryService;
        }

        public override async Task ExecuteAsync(object parameter)
        {
            var result = await _categoryService.GetAllCategories();

            foreach (var item in result)
            {   
                _categoryStore.Categories.Add(item);
            }

        }
    }
}
