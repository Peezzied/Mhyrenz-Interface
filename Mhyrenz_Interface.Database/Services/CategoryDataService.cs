using Mhyrenz_Interface.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Database.Services
{
    public class CategoryDataService: GenericDataService<Category>, ICategoryDataService
    {
        private readonly InventoryDbContextFactory _contextFactory;
        public CategoryDataService(InventoryDbContextFactory contextFactory) : base(contextFactory)
        {
            _contextFactory = contextFactory;
        }
        public override async Task<Category> Get(int id)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                Category entity = await context.Categories
                    .Include(a => a.Products)
                    .FirstOrDefaultAsync((e) => e.Id == id);
                return entity;
            }
        }
        public override async Task<IEnumerable<Category>> GetAll()
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                IEnumerable<Category> entity = await context.Categories
                    .Include(a => a.Products)
                    .ToListAsync();
                return entity;
            }
        }

        public Task<Category> GetByName(string name)
        {
            throw new NotImplementedException();
        }
    }
}
