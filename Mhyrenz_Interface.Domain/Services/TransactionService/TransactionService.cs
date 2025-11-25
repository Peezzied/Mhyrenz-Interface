using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Exceptions;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.State;

namespace Mhyrenz_Interface.Domain.Services.TransactionService
{
    public class TransactionService : ITransactionsService
    {
        private readonly ITransactionsDataService _transactionsDataService;
        private readonly ISessionDataService _sessionDataService;
        private readonly ISessionStore _store;

        public TransactionService(
            ITransactionsDataService transactionsDataService,
            ISessionDataService sessionDataService,
            ISessionStore store)
        {
            _transactionsDataService = transactionsDataService;
            _store = store;
            _sessionDataService = sessionDataService;
        }

        public async Task<Product> Add(Product product, DateTime date, int amount = 1, bool withRecent = false)
        {
            var detachedEntity = product.Clone();

            if (amount < 0)
                throw new NegativeException(amount, product);

            if (product.Qty <= 0 || product.NetQty <= 0)
                throw new InsufficientQuantityException(product.Qty, product.NetQty, product);

            if (product.NetQty - amount < 0)
                throw new InsufficientQuantityException(product.NetQty, amount, product);

            var lastItem = withRecent ? _transactionsDataService.GetLast() : default;
            var isNew = lastItem != null && (int)lastItem?.ProductId == (int)product.Id;
            var newGuid = Guid.NewGuid();

            var session = _store.CurrentSession;

            if (session is null)
                throw new InvalidSession(session);

            var newTransactions = Enumerable.Range(0, amount)
                .Select(_ => new Transaction
                {
                    ProductId = (int)product.Id,
                    UniqueId = isNew ? lastItem.UniqueId : newGuid,
                    Timestamp = date,
                    SessionId = session.Id
                });

            _transactionsDataService.CreateMany(newTransactions);


            return await Task.FromResult(detachedEntity);
        }

        public async Task<IEnumerable<Transaction>> Subtract(Product product, int amount = 1)
        {
            var transactions = _transactionsDataService.GetLatestsByProduct((int)product.Id);

            var matching = transactions
                .Take(amount)
                .ToList();

            _transactionsDataService.DeleteMany(matching);

            return await Task.FromResult(matching);
        }

        public async Task Clear()
        {
            await Task.Run(async () =>
            {
                var transactions = await GetLatests();

                _transactionsDataService.DeleteMany(transactions);

                _transactionsDataService.Clean();
            });
        }

        public async Task<IEnumerable<Transaction>> GetLatests()
        {
            return await Task.Run(() => _transactionsDataService.GetLatests());
        }

        public async Task<bool> RemoveAll()
        {
            return await Task.Run(() =>
            {
                var transactions = _transactionsDataService.GetLatests();

                if (transactions.Any())
                    _transactionsDataService.DeleteMany(transactions);

                return true;
            });
        }
    }
}
