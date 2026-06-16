using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Models.Snapshots;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.SessionService;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Mhyrenz_Interface.Database.Services
{
    public class DatabaseSnapshotService: IDatabaseSnapshotService
    {
        private readonly InventoryDbContextFactory _inventoryDbContextFactory;
        private readonly string _pathDir;

        public DatabaseSnapshotService(InventoryDbContextFactory inventoryDbContextFactory, string pathDir)
        {
            _inventoryDbContextFactory = inventoryDbContextFactory;
            _pathDir = pathDir;
        }

        public async Task ExportSnapshot(Session session, bool isBackup = false)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var snapshot = new DatabaseSnapshot
                {
                    Products = await context.Products
                        .Select(p => new ProductSnapshot
                        {
                            Id = p.Id,
                            Name = p.Name,
                            RetailPrice = p.RetailPrice,
                            Qty = p.Qty,
                            Batch = p.Batch,
                            Expiry = p.Expiry,
                            CostPrice = p.CostPrice,
                            Barcode = p.Barcode,
                            CategoryId = p.CategoryId,
                            Category = p.Category.Name
                        })
                        .ToListAsync(),

                    Sales = await context.Sales
                        .Select(s => new SaleSnapshot
                        {
                            Id = s.Id,
                            SubTotal = s.SubTotal,
                            Total = s.Total,
                            Paid = s.Paid,
                            CreatedAt = s.Created_at,
                            Completed_at = s.Completed_at
                        })
                        .ToListAsync(),

                    Transactions = await context.Transactions
                        .Select(t => new TransactionSnapshot
                        {
                            ProductId = t.ProductId,
                            SaleId = t.SaleId,
                            Amount = t.Amount,
                            RetailPrice = t.RetailPrice,
                            CostPrice = t.CostPrice,
                            DiscountRate = t.DiscountRate
                        })
                        .ToListAsync()
                };

                var json = JsonConvert.SerializeObject(snapshot, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Include
                });

                if (!File.Exists(_pathDir))
                    Directory.CreateDirectory(_pathDir);

                File.WriteAllText(Path.Combine(_pathDir, 
                    (isBackup ? "backup-" : string.Empty) + Session.GenerateCode(session.Id, session.Period) + ".json"), json);
            }
        }

        public async Task RestoreSnapshot(DatabaseSnapshot snapshot)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            using (var tx = await context.Database.BeginTransactionAsync())
            {
                if (snapshot == null)
                    throw new ArgumentNullException("Invalid snapshot");

                try
                {
                    var productMap = await RestoreProducts(context, snapshot.Products);
                    var saleMap = await RestoreSales(context, snapshot.Sales);

                    await RestoreTransactions(context, snapshot.Transactions, productMap, saleMap);

                    await context.SaveChangesAsync();
                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }
        }

        private static async Task<Dictionary<int, int>> RestoreProducts(InventoryDbContext context, List<ProductSnapshot> products)
        {
            var map = new Dictionary<int, int>();

            foreach (var p in products)
            {
                var existing = await context.Products
                    .FirstOrDefaultAsync(x => x.Id == p.Id);

                if (existing == null)
                {
                    existing = new Product
                    {
                        Id = p.Id,
                        Name = p.Name,
                        RetailPrice = p.RetailPrice,
                        Qty = p.Qty,
                        Batch = p.Batch,
                        Expiry = p.Expiry,
                        CostPrice = p.CostPrice,
                        Barcode = p.Barcode,
                        CategoryId = p.CategoryId
                    };

                    context.Products.Add(existing);
                }

                map[p.Id] = p.Id;
            }

            await context.SaveChangesAsync();
            return map;
        }

        private static async Task<Dictionary<int, int>> RestoreSales(InventoryDbContext context, List<SaleSnapshot> sales)
        {
            var map = new Dictionary<int, int>();

            foreach (var s in sales)
            {
                var existing = await context.Sales
                    .FirstOrDefaultAsync(x => x.Id == s.Id);

                if (existing == null)
                {
                    existing = new Sale
                    {
                        Id = s.Id,
                        SubTotal = s.SubTotal,
                        Total = s.Total,
                        Paid = s.Paid,
                        Created_at = s.CreatedAt,
                        Completed_at = s.Completed_at
                    };

                    context.Sales.Add(existing);
                }

                map[s.Id] = s.Id;
            }

            await context.SaveChangesAsync();
            return map;
        }

        private static async Task RestoreTransactions(InventoryDbContext context, List<TransactionSnapshot> transactions, Dictionary<int, int> productMap, Dictionary<int, int> saleMap)
        {
            foreach (var t in transactions)
            {
                context.Transactions.Add(new Transaction
                {
                    ProductId = productMap[t.ProductId],
                    SaleId = t.SaleId.HasValue ? (int?)saleMap[t.SaleId.Value] : null,
                    Amount = t.Amount,
                    RetailPrice = t.RetailPrice,
                    CostPrice = t.CostPrice,
                    DiscountRate = t.DiscountRate
                });
            }
        }

        public DatabaseSnapshot LoadSnapshot()
        {
            var json = File.ReadAllText(_pathDir);

            return JsonConvert.DeserializeObject<DatabaseSnapshot>(json);
        }
    }
}
