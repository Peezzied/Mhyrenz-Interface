using Mhyrenz_Interface.Domain.Exceptions;
using Mhyrenz_Interface.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services.TransactionService
{
    public class TransactionService: ITransactionsService
    {
        private readonly ITransactionsDataService _transactionsDataService;

        public TransactionService(ITransactionsDataService transactionsDataService)
        {
            _transactionsDataService = transactionsDataService;
        }

        public async Task<Product> Add(Product product, int amount = 1)
        {
            var detachedEntity = product.Clone();

            if (amount < 0)
                throw new NegativeException(amount, product);

            if (product.Qty <= 0 || product.NetQty <= 0)
                throw new InsufficientQuantityException(product.Qty, product.NetQty, product);

            if (product.NetQty - amount < 0)
                throw new InsufficientQuantityException(product.NetQty, amount, product);

            var lastItem = await _transactionsDataService.GetLast();
            var isNew = lastItem != null && lastItem?.ProductId == product.Id;
            var newGuid = Guid.NewGuid();

            var newTransactions = Enumerable.Range(0, amount)
                .Select(_ => new Transaction
                {
                    ProductId = product.Id,
                    UniqueId = isNew ? lastItem.UniqueId : newGuid,
                    CreatedAt = DateTime.Now
                });

            await _transactionsDataService.CreateMany(newTransactions);


            return detachedEntity;
        }

        public async Task<Product> Remove(Product product, int amount = 1)
        {
            if (amount <= 0)
                throw new NegativeException(amount, product);

            if (product.Qty <= 0 || product.NetQty <= 0)
                throw new InsufficientQuantityException(product.Qty, product.NetQty, product);

            var transactions = await _transactionsDataService.GetLatestsByProduct(product.Id);

            var matching = transactions
                .Take(amount)
                .ToList();

            await _transactionsDataService.DeleteMany(matching);

            return product;
        }
    }
}
