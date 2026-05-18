using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Mhyrenz_Interface.Database.Services
{
    public class ProductDataService : IProductDataService
    {
        private readonly InventoryDbContextFactory _contextFactory;
        private readonly ICategoryDataService _categoryDataService;
        private readonly ITransactionsDataService _transactionsDataService;

        public ProductDataService(InventoryDbContextFactory contextFactory, ICategoryDataService categoryDataService, ITransactionsDataService transactionsDataService)
        {
            _contextFactory = contextFactory;
            _categoryDataService = categoryDataService;
            _transactionsDataService = transactionsDataService;
        }

        public async Task<Product> Get(int id)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                return await LoadProducts(context)
                    .FirstOrDefaultAsync((e) => e.Id == id); ;
            }
        }

        public async Task<IReadOnlyList<Product>> GetAll()
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                return await LoadProducts(context)
                    .ToListAsync(); ;
            }
        }

        public async Task<IReadOnlyList<Product>> GetAllWithIgnore()
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                return await LoadProducts(context)
                    .IgnoreQueryFilters()
                    .ToListAsync();
            }
        }

        public async Task<Product> Create(Product entity)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                var result = await context.Products.AddAsync(entity);
                await context.SaveChangesAsync();

                return result.Entity;
            }
        }

        public async Task<IReadOnlyList<Product>> CreateMany(IEnumerable<Product> entities)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var result = entities.ToList();
                await context.Products.AddRangeAsync(result);
                await context.SaveChangesAsync();
                return result;
            }
        }

        public async Task DeleteMany(IEnumerable<Product> entities)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                context.Products.RemoveRange(entities);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Product> Update(int id, Product updatedEntity)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                updatedEntity.Id = id;
                context.Products.Update(updatedEntity);
                await context.SaveChangesAsync();
                return updatedEntity;
            }
        }

        public async Task Delete(int id)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                var entity = await LoadProducts(context)
                    .FirstOrDefaultAsync((e) => e.Id == id);
                context.Products.Remove(entity);
                await context.SaveChangesAsync();
            }
        }

        private static Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Product, Category> LoadProducts(InventoryDbContext context)
        {
            return context.Products
                .Include(a => a.Transactions)
                .Include(a => a.Category);
        }

        public async Task<Product> MarkChanged(Product product)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                context.Products.Update(product);
                await context.SaveChangesAsync();
                return product;
            }
        }

        public async Task<IReadOnlyList<Product>> MarkChangedRange(IEnumerable<Product> products)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                context.Products.UpdateRange(products);
                await context.SaveChangesAsync();
                return products.ToList();
            }
        }
    }
}
