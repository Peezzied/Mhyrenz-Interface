using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.Services.SessionService;
using Mhyrenz_Interface.Domain.Services.TransactionService;
using Microsoft.EntityFrameworkCore;

namespace Mhyrenz_Interface.Database.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly InventoryDbContextFactory _inventoryDbContextFactory;
        private readonly ISessionService _sessionService;

        public CheckoutService(InventoryDbContextFactory inventoryDbContextFactory, ISessionService sessionService)
        {
            _inventoryDbContextFactory = inventoryDbContextFactory;
            _sessionService = sessionService;
        }

        public async Task<bool> HasTransactions()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var session = await _sessionService.GetSession();

                if (session == null)
                    return false;

                return await context.Transactions
                    .Include(t => t.Session)
                    .AnyAsync(x => x.Session.Id == session.Id);
            }
        }

        public async Task<CheckoutResult> AddItem(int saleId, int productId, int amount = 1)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var sale = await context.Sales
                    .Include(s => s.Transactions)
                    .Include(s => s.Session)
                    .FirstOrDefaultAsync(s => s.Id == saleId)
                    ?? throw new InvalidOperationException("Sale not found.");

                var product = await context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId)
                    ?? throw new InvalidOperationException("Product not found.");

                var transaction = sale.AddItem(product, sale.SessionId, amount);

                await context.SaveChangesAsync();

                await context.Entry(transaction)
                    .Reference(t => t.Product)
                    .Query()
                    .Include(t => t.Category)
                    .LoadAsync();

                await LoadSale(context, sale);

                await ApplyProductPurchase(context, new List<Sale> { sale });

                return new CheckoutResult
                {
                    Sale = sale,
                    Transaction = transaction
                };
            }
        }


        public async Task<int> InactiveTransactionsCount()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var session = await _sessionService.GetSession();
                return await context.Transactions
                    .Where(t => t.SessionId != session.Id)
                    .CountAsync();
            }
        }

        public async Task<Product> AddItem(int productId, Guid sessionId, int amount = 1)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var product = await context.Products
                    .Include(p => p.Transactions)
                    .FirstOrDefaultAsync(p => p.Id == productId)
                    ?? throw new InvalidOperationException("Product not found.");

                product.AddItem(amount, sessionId);
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
                    .Include(s => s.Session)
                    .FirstOrDefaultAsync(s => s.Id == saleId)
                    ?? throw new InvalidOperationException("Sale not found.");

                var transaction = await context.Transactions
                    .Include(t => t.Product)
                    .Where(t => t.Session.Id == sale.Session.Id)
                    .FirstOrDefaultAsync(t =>
                        t.Id == transactionId &&
                        t.SaleId == saleId)
                    ?? throw new InvalidOperationException("Transaction not found.");

                var resultTransaction = sale.SubtractItem(transaction, amount);

                if (resultTransaction == null)
                {
                    context.Transactions.Remove(transaction);
                }

                await context.SaveChangesAsync();

                await LoadSale(context, sale);

                await ApplyProductPurchase(context, new List<Sale> { sale });

                var checkoutResult = new CheckoutResult
                {
                    Sale = sale,
                    Transaction = resultTransaction,
                };

                if (resultTransaction == null)
                {
                    checkoutResult.WasRemoved = true;
                    checkoutResult.Transaction = transaction;
                }
                else
                {
                    await context.Entry(resultTransaction)
                       .Reference(t => t.Product)
                       .Query()
                       .Include(p => p.Category)
                       .LoadAsync();
                }

                return checkoutResult;
            }
        }

        public async Task<Product> Subtract(int productId, Guid sessionId, int amount = 1)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var product = await context.Products
                    .Include(p => p.Transactions)
                    .FirstOrDefaultAsync(p => p.Id == productId)
                    ?? throw new InvalidOperationException("Product not found.");

                var resultTransaction = product.SubtractItem(sessionId, amount);

                if (resultTransaction.Amount == 0)
                {
                    context.Transactions.Remove(resultTransaction);
                }

                product.RecalculatePurchase();

                await context.SaveChangesAsync();

                return product;
            }
        }

        public async Task<IReadOnlyList<Sale>> GetActive()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var sales = await context.Sales
                .AsNoTracking()
                .Include(s => s.Transactions)
                    .ThenInclude(t => t.Product)
                        .ThenInclude(p => p.Category)
                .Include(s => s.Transactions)
                    .ThenInclude(t => t.Product)
                        .ThenInclude(p => p.PharmaDetails)
                .Where(s => s.Completed_at == null)
                .ToListAsync();

                await ApplyProductPurchase(context, sales);

                return sales;
            }
        }

        public async Task<IReadOnlyList<Sale>> GetHistory()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                return await context.Sales
                    .AsNoTracking()
                    .Include(s => s.Transactions)
                        .ThenInclude(t => t.Product)
                    .Where(s => s.Completed_at != null)
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

        public async Task<Sale> CompleteSale(int saleId, decimal received)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var sale = await context.Sales
                    .Where(s => s.Completed_at == null)
                    .Include(s => s.Transactions)
                    .FirstOrDefaultAsync(s => s.Id == saleId)
                    ?? throw new InvalidOperationException("Sale not found.");

                if (!sale.Transactions.Any())
                    throw new InvalidOperationException("Cannot complete an empty sale.");

                sale.Completed_at = DateTime.Now;
                sale.ReceiveCash(received);

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

        public async Task<CheckoutResult> ApplyDiscount(DiscountInfo discountInfo, int saleId, int transactionId)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var sale = await context.Sales
                    .Include(s => s.Transactions)
                    .Include(s => s.Session)
                    .FirstOrDefaultAsync(s => s.Id == saleId)
                    ?? throw new InvalidOperationException("Sale not found.");

                var transaction = await context.Transactions
                    .Include(t => t.Product)
                    .Where(t => t.Session.Id == sale.Session.Id)
                    .FirstOrDefaultAsync(t =>
                        t.Id == transactionId &&
                        t.SaleId == saleId)
                    ?? throw new InvalidOperationException("Transaction not found.");

                sale.Discount = discountInfo.Discount;
                transaction.Discount = discountInfo.Discount;
                transaction.DiscountRate = discountInfo.DiscountRate;

                sale.RecalculateTotals();

                await context.SaveChangesAsync();

                return new CheckoutResult
                {
                    Sale = sale,
                    Transaction = transaction
                };
            }
        }

        public async Task<Transaction> Update(int id, Transaction product)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                product.Id = id;

                context.Transactions.Update(product);

                await context.SaveChangesAsync();

                return product;
            }
        }

        private static async Task LoadSale(InventoryDbContext context, Sale sale)
        {
            await context.Entry(sale)
                .Collection(s => s.Transactions)
                .Query()
                .Include(t => t.Product)
                .LoadAsync();
        }
        private static async Task ApplyProductPurchase(InventoryDbContext context, List<Sale> sales)
        {
            var productIds = sales
                .SelectMany(s => s.Transactions)
                .Select(t => t.ProductId)
                .Distinct()
                .ToList();

            var purchasesByProductId = await context.Transactions
                .AsNoTracking()
                .Where(t => productIds.Contains(t.ProductId))
                .GroupBy(t => t.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Purchase = g.Sum(t => t.Amount)
                })
                .ToDictionaryAsync(x => x.ProductId, x => x.Purchase);

            foreach (var transaction in sales.SelectMany(s => s.Transactions))
            {
                if (purchasesByProductId.TryGetValue(transaction.ProductId, out var purchase))
                {
                    transaction.Product.Purchase = purchase;
                }
            }
        }

    }
}
