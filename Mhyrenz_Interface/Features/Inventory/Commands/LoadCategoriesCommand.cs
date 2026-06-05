using System.Threading.Tasks;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Domain.Services.CategoryService;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Inventory.Commands
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
            //var result = await _categoryService.GetAllCategories();

            //foreach (var item in result)
            //{
            //    _categoryStore.Categories[item] = new ListCollectionView(_inventoryStore.Products)
            //    {
            //        Filter = (obj) => obj is ProductDataViewModel vm
            //            && vm.CategoryId == item.Id
            //    };
            //}

        }
    }
}
