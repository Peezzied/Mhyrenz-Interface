using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Mhyrenz_Interface.Database.Services
{
    public class CategoryDataService : ICategoryDataService
    {
        private readonly InventoryDbContextFactory _contextFactory;

        public CategoryDataService(InventoryDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Category> Create(Category entity)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                var result = await context.Categories.AddAsync(entity);
                await context.SaveChangesAsync();

                return result.Entity;
            }
        }

        public async Task Delete(int id)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                var entity = await context.Categories
                    .FirstOrDefaultAsync((e) => e.Id == id);
                context.Categories.Remove(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Category> Get(int id)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                Category entity = await context.Categories
                    .FirstOrDefaultAsync((e) => e.Id == id);
                return entity;
            }
        }

        public async Task<IReadOnlyList<Category>> GetAll()
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                return await context.Categories
                    .ToListAsync();
            }
        }

        public async Task<Category> Update(int id, Category updatedEntity)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                updatedEntity.Id = id;
                context.Categories.Update(updatedEntity);
                await context.SaveChangesAsync();
                return updatedEntity;
            }
        }
    }
}
