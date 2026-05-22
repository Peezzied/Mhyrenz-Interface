using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.Services.TransactionService;
using Microsoft.EntityFrameworkCore;

namespace Mhyrenz_Interface.Database.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly InventoryDbContextFactory _inventoryDbContextFactory;

        public CheckoutService(InventoryDbContextFactory inventoryDbContextFactory)
        {
            _inventoryDbContextFactory = inventoryDbContextFactory;
        }

        public async Task<CheckoutResult> AddItem(int saleId, int productId, DiscountInfo discountInfo, int amount = 1)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var sale = await context.Sales
                    .Include(s => s.Transactions)
                    .FirstOrDefaultAsync(s => s.Id == saleId)
                    ?? throw new InvalidOperationException("Sale not found.");

                var product = await context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId)
                    ?? throw new InvalidOperationException("Product not found.");

                var transaction = sale.AddItem(product, discountInfo, amount);

                await context.SaveChangesAsync();

                await context.Entry(transaction)
                    .Reference(t => t.Item)
                    .LoadAsync();

                await context.Entry(sale)
                    .Collection(s => s.Transactions)
                    .Query()
                    .Include(t => t.Item)
                    .LoadAsync();

                return new CheckoutResult
                {
                    Sale = sale,
                    Transaction = transaction
                };
            }
        }

        public async Task<Product> AddItem(int productId, int amount = 1)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var product = await context.Products
                    .Include(p => p.Transactions)
                    .FirstOrDefaultAsync(p => p.Id == productId)
                    ?? throw new InvalidOperationException("Product not found.");

                product.AddItem(amount);
                product.RecalculatePurchase();

                await context.SaveChangesAsync();

                return product;
            }
        }

        public async Task<CheckoutResult> Subtract(int saleId, int transactionId, int amount = 1)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var sale = await context.Sales
                    .Include(s => s.Transactions)
                    .FirstOrDefaultAsync(s => s.Id == saleId)
                    ?? throw new InvalidOperationException("Sale not found.");

                var transactions = await context.Transactions
                    .Include(t => t.Item)
                    .FirstOrDefaultAsync(t =>
                        t.Id == transactionId &&
                        t.SaleId == saleId)
                    ?? throw new InvalidOperationException("Transaction not found.");

                transactions = sale.SubtractItem(transactions, amount);

                await context.SaveChangesAsync();

                await context.Entry(sale)
                    .Collection(s => s.Transactions)
                    .Query()
                    .Include(t => t.Item)
                    .LoadAsync();

                if (transactions != null)
                {
                    await context.Entry(transactions)
                        .Reference(t => t.Item)
                        .LoadAsync();
                }

                return new CheckoutResult
                {
                    Sale = sale,
                    Transaction = transactions
                };
            }
        }

        public async Task<Product> Subtract(int productId, int amount = 1)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var product = await context.Products
                    .Include(p => p.Transactions)
                    .FirstOrDefaultAsync(p => p.Id == productId)
                    ?? throw new InvalidOperationException("Product not found.");

                product.SubtractItem(amount);
                product.RecalculatePurchase();

                await context.SaveChangesAsync();

                return product;
            }
        }

        public async Task<IReadOnlyList<Sale>> GetActive()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                return await context.Sales
                    .AsNoTracking()
                    .Include(s => s.Transactions)
                        .ThenInclude(t => t.Item)
                            .ThenInclude(i => i.Category)
                    .Include(s => s.Transactions)
                        .ThenInclude(t => t.Item)
                            .ThenInclude(i => i.PharmaDetails)
                    .Where(s => s.Completed_at == null)
                    .ToListAsync();
            }
        }

        public async Task DiscardSale(int saleId)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var sale = await context.Sales
                    .Include(s => s.Transactions)
                    .FirstOrDefaultAsync(s => s.Id == saleId)
                    ?? throw new InvalidOperationException("Sale not found.");

                context.Transactions.RemoveRange(sale.Transactions);
                context.Sales.Remove(sale);

                await context.SaveChangesAsync();
            }
        }

        public async Task<Sale> CompleteSale(int saleId)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var sale = await context.Sales
                    .Include(s => s.Transactions)
                    .FirstOrDefaultAsync(s => s.Id == saleId)
                    ?? throw new InvalidOperationException("Sale not found.");

                if (!sale.Transactions.Any())
                    throw new InvalidOperationException("Cannot complete an empty sale.");

                sale.Completed_at = DateTime.Now;

                await context.SaveChangesAsync();

                return sale;
            }
        }

        public async Task<Sale> Create(Guid sessionId)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var sale = new Sale
                {
                    Created_at = DateTime.Now,
                    SessionId = sessionId
                };
                context.Sales.Add(sale);
                await context.SaveChangesAsync();

                return sale;
            }
        }
    }
}
