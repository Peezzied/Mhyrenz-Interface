using System.Collections.Generic;
using System.Threading.Tasks;
using Mhyrenz_Interface.Database;
using Mhyrenz_Interface.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Mhyrenz_Interface.Domain.Services.CategoryService
{
    public class CategoryService : ICategoryService
    {
        private readonly InventoryDbContextFactory _inventoryDbContextFactory;

        public CategoryService(InventoryDbContextFactory inventoryDbContextFactory)
        {
            _inventoryDbContextFactory = inventoryDbContextFactory;
        }

        public async Task<Category> Get(int id)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var entity = await context.Categories
                    .FindAsync(id);
                return entity;
            }
        }

        public async Task<IEnumerable<Category>> GetAllCategories()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                return await context.Categories
                    .ToListAsync();
            }
        }
    }
}
