using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using Mhyrenz_Interface.Database;
using Mhyrenz_Interface.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Mhyrenz_Interface.Domain.Services.ProductService
{
    public class ProductService : IProductService
    {
        private readonly InventoryDbContextFactory _inventoryDbContextFactory;

        public ProductService(InventoryDbContextFactory inventoryDbContextFactory)
        {
            _inventoryDbContextFactory = inventoryDbContextFactory;
        }

        public async Task<Product> Create(Product entity)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                context.Products.Add(entity);
                await context.SaveChangesAsync();

                return entity;
            }
        }

        public async Task<IEnumerable<Product>> AddMany(IEnumerable<Product> entities)
        {
            throw new System.NotImplementedException(); // TODO: implement this once bulk add is supported in the UI
        }

        public async Task<Product> Get(int id)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var product = await context.Products
                    .Include(p => p.Supplier)
                    .Include(p => p.Category)
                    .Include(p => p.PharmaDetails)
                    .FirstOrDefaultAsync(p => p.Id == id) ?? throw new KeyNotFoundException($"Product with id {id} not found.");

                var purchase = await context.Transactions
                    .Where(t => t.ProductId == id)
                    .SumAsync(t => t.Amount);
                product.Purchase = purchase;

                return product;
            }
        }

        public async Task<IReadOnlyList<Product>> GetAll()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var products = await context.Products
                    .AsNoTracking()
                    .Include(p => p.Supplier)
                    .Include(p => p.Category)
                    .Include(p => p.PharmaDetails)
                    .ToListAsync();

                var purchases = await GetTransactions(context);

                foreach (var product in products)
                {
                    product.Purchase = purchases.TryGetValue(product.Id, out var purchase)
                        ? purchase
                        : 0;
                }

                return products;
            }
        }

        private static async Task<Dictionary<int, int>> GetTransactions(InventoryDbContext context)
        {
            return await context.Transactions
                .IgnoreQueryFilters()
                .AsNoTracking()
                .GroupBy(t => t.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Purchase = g.Sum(t => t.Amount)
                })
                .ToDictionaryAsync(x => x.ProductId, x => x.Purchase);
        }

        public async Task<IReadOnlyList<Product>> RemoveMany(IEnumerable<int> productIds)
        {
            return await SoftRemove(productIds, p => p.Delete());
        }

        public async Task<IReadOnlyList<Product>> RemoveManyBack(IEnumerable<int> productIds)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var purchases = await GetTransactions(context);
                return await SoftRemove(productIds, p =>
                {
                    p.DeleteBack();
                    p.Purchase = purchases.TryGetValue(p.Id, out var purchase)
                        ? purchase
                        : 0;
                });
            }
        }

        private async Task<IReadOnlyList<Product>> SoftRemove(IEnumerable<int> productIds, Action<Product> action)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var ids = productIds.ToList();

                var products = await context.Products
                    .IgnoreQueryFilters()
                    .Include(p => p.Category)
                    .Where(p => ids.Contains(p.Id))
                    .ToListAsync();

                foreach (var product in products)
                {
                    action.Invoke(product);
                }

                await context.SaveChangesAsync();

                return products;
            }
        }

        public async Task<Product> Update(int id, Product product)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                product.Id = id;

                context.Products.Update(product);

                await context.SaveChangesAsync();

                return product;
            }
        }

        public async Task<int> RemovePhysical()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var entities = await context.Products
                    .IgnoreQueryFilters()
                    .Where(p => p.IsDeleted)
                    .ToListAsync();

                context.Products.RemoveRange(entities);

                await context.SaveChangesAsync();

                return entities.Count;
            }
        }
    }
}
