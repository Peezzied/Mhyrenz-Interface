using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Transaction = Mhyrenz_Interface.Domain.Models.Transaction;

namespace Mhyrenz_Interface.Database.Services
{
    public class TransactionsDataService : GenericDataService<Transaction>, ITransactionsDataService
    {
        private readonly InventoryDbContextFactory _contextFactory;

        public TransactionsDataService(InventoryDbContextFactory contextFactory) : base(contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task Clean()
        {
            await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    context.DropCollection(Name);
                    context.GetCollection<Transaction>(Name);
                }
            });
        }

        public override async Task<IEnumerable<Transaction>> GetAll()
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var col = context.GetCollection<Transaction>(Name);

                    var transactions = col.FindAll().ToList();

                    LoadReferences(context, transactions);

                    return transactions;
                }
            });
        }

        public async Task<IEnumerable<Transaction>> GetLatestsByProduct(int productId)
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var col = context.GetCollection<Transaction>(Name);

                    var list = col.Query()
                        .Where(t => t.ProductId == productId)
                        .OrderByDescending(t => t.Timestamp)
                        .ToList();

                    LoadReferences(context, list);

                    return list;
                }
            });
        }

        public async Task<Transaction> GetLast()
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var col = context.GetCollection<Transaction>(Name);

                    var trx = col.Query()
                        .OrderByDescending(t => t.Timestamp)
                        .FirstOrDefault();

                    if (trx != null)
                        LoadReference(context, trx);

                    return trx;
                }
            });
        }

        public async Task<IEnumerable<Transaction>> GetLatests()
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var col = context.GetCollection<Transaction>(Name);

                    var list = col.Query()
                        .OrderByDescending(t => t.Timestamp)
                        .ToList();

                    LoadReferences(context, list);

                    return list;
                }
            });
        }

        // --------------------------
        // Manual navigation loading
        // --------------------------

        private void LoadReferences(ILiteDatabase context, List<Transaction> list)
        {
            foreach (var transaction in list)
                LoadReference(context, transaction);
        }

        private void LoadReference(ILiteDatabase context, Transaction transaction)
        {
            if (transaction == null) return;

            transaction.Item = context
                .GetCollection<Product>(nameof(Product).TableName())
                .FindById(transaction.ProductId);

            transaction.Session = context
                .GetCollection<Session>(nameof(Session).TableName())
                .FindById(transaction.SessionId);
        }
    }
}
