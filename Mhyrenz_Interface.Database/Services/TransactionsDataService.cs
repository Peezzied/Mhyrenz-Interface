using System.Collections.Generic;
using System.Linq;
using LiteDB;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Transaction = Mhyrenz_Interface.Domain.Models.Transaction;

namespace Mhyrenz_Interface.Database.Services
{
    public class TransactionsDataService : GenericDataService<Transaction>, ITransactionsDataService
    {
        private readonly InventoryDbService _context;

        public TransactionsDataService(InventoryDbService context) : base(context)
        {
            _context = context;
        }

        public void Clean()
        {
            _context.Instance.DropCollection(Name);
            GetTable();
        }

        public new IEnumerable<Transaction> GetAll()
        {
            var transactions = GetTable().FindAll().ToList();

            LoadReferences(transactions);

            return transactions;
        }

        public IEnumerable<Transaction> GetLatestsByProduct(int productId)
        {
            var list = GetTable().Query()
                .Where(t => t.ProductId == productId)
                .OrderByDescending(t => t.Timestamp)
                .ToList();

            LoadReferences(list);

            return list;
        }

        public Transaction GetLast()
        {
            var transaction = GetTable().Query()
                .OrderByDescending(t => t.Timestamp)
                .FirstOrDefault();

            if (transaction != null)
            {
                var context = _context.Instance;
                transaction.Item = context
                    .GetCollection<Product>(typeof(Product).TableName())
                    .FindById(transaction.ProductId);

                transaction.Session = context
                    .GetCollection<Session>(typeof(Session).TableName())
                    .FindById(transaction.SessionId);
            }


            return transaction;
        }

        public IEnumerable<Transaction> GetLatests()
        {
            var list = GetTable().Query()
                .OrderByDescending(t => t.Timestamp)
                .ToList();

            LoadReferences(list);

            return list;
        }

        // --------------------------
        // Manual navigation loading
        // --------------------------

        private void LoadReferences(List<Transaction> list)
        {
            var context = _context.Instance;

            // 1. Get all product IDs we need
            var productIds = list.Select(t => t.ProductId).Distinct().ToHashSet();
            var products = context
                .GetCollection<Product>(typeof(Product).TableName())
                .Find(p => productIds.Contains(p.Id))
                .ToDictionary(p => p.Id);

            // 2. Get all session IDs we need
            var sessionIds = list.Select(t => t.SessionId).Distinct().ToHashSet();
            var sessions = context
                .GetCollection<Session>(typeof(Session).TableName())
                .Find(s => sessionIds.Contains(s.Id))
                .ToDictionary(s => s.Id);

            // 3. Assign references in-memory
            foreach (var transaction in list)
            {
                products.TryGetValue(transaction.ProductId, out var product);
                sessions.TryGetValue(transaction.SessionId, out var session);

                transaction.Item = product;
                transaction.Session = session;
            }
        }

        public IEnumerable<Transaction> GetAllRaw()
        {
            return base.GetAll();
        }
    }
}
