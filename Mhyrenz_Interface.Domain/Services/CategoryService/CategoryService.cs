using Mhyrenz_Interface.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services.CategoryService
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryDataService _categoryDataService;
        public CategoryService(ICategoryDataService categoryDataService)
        {
            _categoryDataService = categoryDataService;
        }

        public async Task<Category> Get(int id)
        {
            return await Task.Run(() => _categoryDataService.Get(id));
        }

        public async Task<IEnumerable<Category>> GetAllCategories()
        {
            return await Task.Run(() => _categoryDataService.GetAll());
        }
    }
}
