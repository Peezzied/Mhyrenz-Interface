using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;
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

        public async Task<CheckoutResult> AddItem(int saleId, int productId, int amount = 1)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            using (var dbTransaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    var sale = await context.Sales
                        .Include(s => s.Transactions)
                        .FirstOrDefaultAsync(s => s.Id == saleId)
                        ?? throw new InvalidOperationException("Sale not found.");

                    var transaction = await context.Transactions
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(t => t.ProductId == productId && t.SaleId == saleId);

                    if (transaction != null)
                    {
                        transaction.IncreaseAmount(amount);
                        transaction.Restore();
                    }
                    else
                    {
                        var product = await context.Products
                            .AsNoTracking()
                            .FirstOrDefaultAsync(p => p.Id == productId)
                            ?? throw new InvalidOperationException("Product not found.");

                        transaction = new Transaction
                        {
                            ProductId = product.Id,
                            Amount = amount,
                            RetailPrice = product.RetailPrice,
                            CostPrice = product.CostPrice
                        };

                        sale.Transactions.Add(transaction);
                    }

                    sale.RecalculateTotals(isFiltered: false);

                    await context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    return new CheckoutResult
                    {
                        Sale = sale,
                        Transaction = transaction
                    };
                }
                catch
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<CheckoutResult> AddItem(int productId, int amount = 1)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            using (var dbTransaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    var product = await context.Products
                        .Include(p => p.Transactions)
                        .FirstOrDefaultAsync(p => p.Id == productId)
                        ?? throw new InvalidOperationException("Product not found.");

                    var transaction = product.AddItem(amount);

                    product.RecalculatePurchase();

                    await context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    transaction.Product = product;

                    return new CheckoutResult
                    {
                        Transaction = transaction
                    };
                }
                catch (Exception)
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<CheckoutResult> Subtract(int saleId, int transactionId, int amount = 1)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            using (var dbTransaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    var sale = await context.Sales
                        .Include(s => s.Transactions)
                        .FirstOrDefaultAsync(s => s.Id == saleId)
                        ?? throw new InvalidOperationException("Sale not found.");

                    var transaction = sale.Transactions.FirstOrDefault(t => t.Id == transactionId)
                        ?? throw new InvalidOperationException("Transaction not found in sale.");

                    transaction.DecreaseAmount(amount);

                    if (transaction.Amount == 0)
                    {
                        transaction.Delete();
                    }

                    sale.RecalculateTotals();

                    await context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    var checkoutResult = new CheckoutResult
                    {
                        Sale = sale,
                        Transaction = transaction,
                    };

                    return checkoutResult;
                }
                catch
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<CheckoutResult> Subtract(int productId, int amount = 1)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            using (var dbTransaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    var product = await context.Products
                        .Include(p => p.Transactions)
                        .FirstOrDefaultAsync(p => p.Id == productId)
                        ?? throw new InvalidOperationException("Product not found.");

                    var transaction = product.SubtractItem(amount);

                    product.RecalculatePurchase();

                    await context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    transaction.Product = product;

                    return new CheckoutResult
                    {
                        Transaction = transaction
                    };
                }
                catch
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<IReadOnlyList<Sale>> GetActiveSales()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var sales = await context.Sales
                    .AsNoTracking()
                    .Include(s => s.Transactions)
                    .Where(s => s.Completed_at == null)
                    .ToListAsync();

                return sales;
            }
        }

        public HashSet<int> GetActiveSalesSet()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                return context.Sales
                    .AsNoTracking()
                    .Where(s => s.Completed_at == null)
                    .Select(s => s.Id)
                    .ToHashSet();
            }
        }

        public async Task<int> GetSaleSequence()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var connection = context.Database.GetDbConnection();

                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT seq FROM sqlite_sequence WHERE name = @name";

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@name";
                    parameter.Value = nameof(context.Sales);
                    command.Parameters.Add(parameter);

                    var result = await command.ExecuteScalarAsync();

                    return result == DBNull.Value || result == null
                        ? 0
                        : Convert.ToInt32(result);
                }
            }
        }

        public async Task<IReadOnlyList<Transaction>> GetAllTransactions()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var transactions = await context.Transactions
                    .AsNoTracking()
                    .ToListAsync();

                return transactions;
            }
        }

        public async Task<bool> HasCompletedSales()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                return await context.Transactions
                    .AsNoTracking()
                    .AnyAsync(t => t.Sale.Completed_at != null);
            }
        }

        public async Task<bool> HasActiveSales()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                return await context.Transactions
                    .AsNoTracking()
                    .AnyAsync(t => t.SaleId != null && t.Sale.Completed_at == null);
            }
        }

        public async Task<IReadOnlyList<Sale>> GetSalesHistory()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                return await context.Sales
                    .AsNoTracking()
                    .Include(s => s.Transactions)
                    .Where(s => s.Completed_at != null)
                    .ToListAsync();
            }
        }

        public async Task DiscardSale(int saleId)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            using (var dbTransaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    var transactions = await context.Transactions
                        .Where(t => t.SaleId.HasValue && t.SaleId.Value == saleId)
                        .ToListAsync();

                    context.Transactions.RemoveRange(transactions);

                    context.Sales.Remove(await context.Sales.FindAsync(saleId));

                    await context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();
                }
                catch
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<Sale> CompleteSale(int saleId, decimal received)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            using (var dbTransaction = await context.Database.BeginTransactionAsync())
            {
                try
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
                    await dbTransaction.CommitAsync();

                    return sale;
                }
                catch
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<Sale> Create(Guid sessionId)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var sale = new Sale
                {
                    Created_at = DateTime.Now
                };
                context.Sales.Add(sale);
                await context.SaveChangesAsync();

                return sale;
            }
        }

        public async Task<DiscountResult> ApplyDiscount(DiscountInfo discountInfo, int saleId, IEnumerable<Transaction> transactions, bool isReversed = false)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            using (var dbTransaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    var sale = await context.Sales
                       .Include(s => s.Transactions)
                       .FirstOrDefaultAsync(s => s.Id == saleId)
                       ?? throw new InvalidOperationException("Sale not found.");

                    var transactionIds = transactions.Select(t => t.Id).ToHashSet();

                    var targetTransactions = sale.Transactions
                        .Where(t => transactionIds.Contains(t.Id))
                        .ToList();

                    if (targetTransactions.Count != transactionIds.Count)
                        throw new InvalidOperationException("One or more transactions were not found.");

                    if (isReversed)
                    {
                        var snapshots = transactions.ToDictionary(t => t.Id);

                        foreach (var transaction in targetTransactions)
                        {
                            var snapshot = snapshots[transaction.Id];

                            if (snapshot.Discount != Discount.None && discountInfo.Discount == Discount.None)
                                transaction.ApplyDiscount(snapshot.DiscountRate);
                            else
                                transaction.RemoveDiscount();

                            transaction.Discount = snapshot.Discount;
                        }
                    }
                    else
                    {
                        foreach (var transaction in targetTransactions)
                        {
                            if (transaction.Discount != Discount.None && discountInfo.Discount == Discount.None)
                                transaction.RemoveDiscount();
                            else
                                transaction.ApplyDiscount(discountInfo.DiscountRate);

                            transaction.Discount = discountInfo.Discount;
                        }
                    }

                    sale.Discount = sale.Transactions
                        .FirstOrDefault(t => t.Discount != Discount.None)?.Discount ?? Discount.None;
                    sale.RecalculateTotals(isFiltered: false);

                    await context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    return new DiscountResult
                    {
                        Sale = sale,
                        Transactions = targetTransactions
                    };
                }
                catch
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task ConvertAgnosticTransactions()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            using (var dbTransaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    var orphanTransactions = await context.Transactions
                    .Where(t => t.SaleId == null)
                    .ToListAsync();

                    if (orphanTransactions.Count == 0)
                        return;

                    var lineTotal = orphanTransactions.Sum(t => t.RetailPrice * t.Amount);

                    var sale = new Sale
                    {
                        Created_at = DateTime.Now,
                        Total = lineTotal,
                        SubTotal = lineTotal
                    };

                    context.Sales.Add(sale);

                    await context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();
                }
                catch
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<Transaction> Update(int id, Transaction transaction)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                transaction.Id = id;

                context.Attach(transaction);
                context.Entry(transaction).State = EntityState.Modified;

                await context.SaveChangesAsync();

                return transaction;
            }
        }

        public async Task RemovePhysically()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var transactions = context.Transactions
                    .IgnoreQueryFilters()
                    .Where(t => t.IsDeleted);

                context.RemoveRange(transactions);

                await context.SaveChangesAsync();
            }
        }

        public async Task<CheckoutResult> MarkRemoveMany(int saleId, IEnumerable<int> transactions, bool isDeleted = true)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            using (var dbTransaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    var ids = transactions.ToHashSet();

                    var affectedTransactions = await context.Transactions
                        .IgnoreQueryFilters()
                        .Where(t => ids.Contains(t.Id) && t.SaleId == saleId)
                        .ToListAsync();

                    foreach (var transaction in affectedTransactions)
                    {
                        if (isDeleted)
                            transaction.Delete();
                        else
                            transaction.Restore();
                    }

                    var sale = await context.Sales
                        .Include(s => s.Transactions)
                        .FirstOrDefaultAsync(s => s.Id == saleId);

                    sale.RecalculateTotals();

                    await context.SaveChangesAsync();

                    await dbTransaction.CommitAsync();

                    return new CheckoutResult
                    {
                        Sale = sale,
                        Transactions = affectedTransactions
                    };
                }
                catch
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}
