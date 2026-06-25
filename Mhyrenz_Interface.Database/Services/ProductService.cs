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
                    .Include(p => p.Category)
                    .Include(p => p.PharmaDetails)
                    .FirstOrDefaultAsync(p => p.Id == id) ?? throw new KeyNotFoundException($"Product with id {id} not found.");

                var totals = await context.Transactions
                    .AsNoTracking()
                    .Where(t => t.ProductId == id)
                    .GroupBy(t => 1)
                    .Select(g => new
                    {
                        Purchase = g.Sum(t => t.Amount),
                        Sales = g.Sum(t => t.LineTotal)
                    })
                    .FirstOrDefaultAsync();

                product.Purchase = totals?.Purchase ?? 0;
                product.Sales = totals?.Sales ?? 0m;

                return product;
            }
        }

        public async Task<IReadOnlyList<Product>> GetAll()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var products = await context.Products
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.PharmaDetails)
                    .ToListAsync();

                var productTotals = await GetProductTotals(context);

                foreach (var p in products)
                {
                    if (productTotals.TryGetValue(p.Id, out var totals))
                    {
                        p.Purchase = totals.Purchase;
                        p.Sales = totals.Sales;
                    }
                    else
                    {
                        p.Purchase = 0;
                        p.Sales = 0;
                    }
                }

                return products;
            }
        }

        public async Task ApplyPurchases()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var products = await context.Products
                   .AsNoTracking()
                   .ToListAsync();

                var productTotals = await GetProductTotals(context);

                foreach (var product in products)
                {
                    if (productTotals.TryGetValue(product.Id, out var totals))
                    {
                        product.ApplyPurchase(totals.Purchase);
                    }
                }

                await context.SaveChangesAsync();
            }
        }

        public async Task<Product> SetMarkupRate(int productId, decimal rate)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var product = await context.Products
                    .Include(p => p.Category)
                    .Include(p => p.PharmaDetails)
                    .FirstOrDefaultAsync(p => p.Id == productId);

                product.SetMarkupRate(rate);

                await context.SaveChangesAsync();

                return product;
            }
        }

        private static async Task<Dictionary<int, (int Purchase, decimal Sales)>> GetProductTotals(InventoryDbContext context)
        {
            var raw = await context.Transactions
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Select(t => new
                {
                    t.ProductId,
                    t.Amount,
                    t.RetailPrice
                })
                .ToListAsync();

            return raw
                .GroupBy(t => t.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => (
                        Purchase: g.Sum(t => t.Amount),
                        Sales: g.Sum(t => t.RetailPrice * t.Amount) // LineTotal formula
                    ));
        }

        public async Task<IReadOnlyList<Product>> RemoveMany(IEnumerable<int> productIds)
        {
            return await SoftRemove(productIds, p => p.Delete());
        }

        public async Task<IReadOnlyList<Product>> RemoveManyBack(IEnumerable<int> productIds)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var productTotals = await GetProductTotals(context);
                return await SoftRemove(productIds, p =>
                {
                    p.DeleteBack();
                    if (productTotals.TryGetValue(p.Id, out var totals))
                    {
                        p.Purchase = totals.Purchase;
                        p.Sales = totals.Sales;
                    }
                    else
                    {
                        p.Purchase = 0;
                        p.Sales = 0;
                    }
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

        public async Task Update(int id, Action<Product> updater)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var product = await context.Products.FindAsync(id);

                if (product == null)
                    return;

                updater(product);

                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsBarcodeUnique(string barcode)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                return !(await context.Products
                    .AnyAsync(p => p.Barcode == barcode));
            }
        }

        public async Task RemovePhysically()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var entities = await context.Products
                    .IgnoreQueryFilters()
                    .Where(p => p.IsDeleted)
                    .ToListAsync();

                if (!entities.Any())
                    return;

                context.Products.RemoveRange(entities);

                await context.SaveChangesAsync();
            }
        }
    }
}
