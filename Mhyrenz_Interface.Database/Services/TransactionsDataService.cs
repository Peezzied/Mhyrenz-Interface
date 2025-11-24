using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task Clean()
        {
            await Task.Run(() =>
            {
                _context.Instance.DropCollection(Name);
                GetTable();
            });
        }

        public override async Task<IEnumerable<Transaction>> GetAll()
        {
            return await Task.Run(() =>
            {
                var transactions = GetTable().FindAll().ToList();

                LoadReferences(transactions);

                return transactions;
            });
        }

        public async Task<IEnumerable<Transaction>> GetLatestsByProduct(int productId)
        {
            return await Task.Run(() =>
            {
                var list = GetTable().Query()
                    .Where(t => t.ProductId == productId)
                    .OrderByDescending(t => t.Timestamp)
                    .ToList();

                LoadReferences(list);

                return list;
            });
        }

        public async Task<Transaction> GetLast()
        {
            return await Task.Run(() =>
            {
                var trx = GetTable().Query()
                    .OrderByDescending(t => t.Timestamp)
                    .FirstOrDefault();

                if (trx != null)
                    LoadReference(trx);

                return trx;
            });
        }

        public async Task<IEnumerable<Transaction>> GetLatests()
        {
            return await Task.Run(() =>
            {
                var list = GetTable().Query()
                    .OrderByDescending(t => t.Timestamp)
                    .ToList();

                LoadReferences(list);

                return list;
            });
        }

        // --------------------------
        // Manual navigation loading
        // --------------------------

        private void LoadReferences(List<Transaction> list)
        {
            foreach (var transaction in list)
                LoadReference(transaction);
        }

        private void LoadReference(Transaction transaction)
        {
            if (transaction == null) return;

            var context = _context.Instance;
            transaction.Item = context
                .GetCollection<Product>(typeof(Product).TableName())
                .FindById(transaction.ProductId);

            transaction.Session = context
                .GetCollection<Session>(typeof(Session).TableName())
                .FindById(transaction.SessionId);
        }
    }
}
