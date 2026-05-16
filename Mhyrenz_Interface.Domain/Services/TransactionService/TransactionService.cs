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
        private readonly ICategoryDataService _categoryDataService;
        private readonly ISessionDataService _sessionDataService;
        private readonly ISessionStore _store;

        public TransactionService(
            ITransactionsDataService transactionsDataService,
            ICategoryDataService categoryDataService,
            ISessionDataService sessionDataService,
            ISessionStore store)
        {
            _transactionsDataService = transactionsDataService;
            _categoryDataService = categoryDataService;
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

            var lastItem = withRecent ? await _transactionsDataService.GetLast() : default;
            var isNew = lastItem != null && (int)lastItem?.ProductId == (int)product.Id;
            var newGuid = Guid.NewGuid();

            var session = _store.CurrentSession;

            if (session is null)
                throw new InvalidSession(session);

            var newTransactions = Enumerable.Range(0, amount)
                .Select(_ => new Transaction
                {
                    ProductId = product.Id,
                    UniqueId = isNew ? lastItem.UniqueId : newGuid,
                    Timestamp = date,
                    SessionId = session.Id
                });

            await _transactionsDataService.CreateMany(newTransactions);

            return detachedEntity;
        }

        public async Task<IEnumerable<Transaction>> Subtract(Product product, int amount = 1)
        {
            var transactions = await _transactionsDataService.GetAllByProduct((int)product.Id);

            var matching = transactions
                .Take(amount)
                .ToList();

            await _transactionsDataService.DeleteMany(matching);

            return await Task.FromResult(matching);
        }

        public async Task Clear()
        {
            var transactions = await GetLatests();

            await _transactionsDataService.DeleteMany(transactions);

            await _transactionsDataService.Clean();
        }

        public async Task<IEnumerable<Transaction>> GetLatests()
        {
            return await Task.Run(() => _transactionsDataService.GetAll());
        }

        public async Task RemoveAll()
        {
            var transactions = await _transactionsDataService.GetAll();

            if (transactions.Any())
                await _transactionsDataService.DeleteMany(transactions);
        }

    }
}
