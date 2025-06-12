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

            for (int i = 0; i < amount; i++)
            {
                await _transactionsDataService.Create(new Transaction()
                {
                    ProductId = product.Id,
                    UniqueId = isNew ? lastItem.UniqueId : newGuid,
                    CreatedAt = DateTime.Now,
                });
            }

            return detachedEntity;
        }

        public async Task<Product> Remove(Product product, int amount = 1)
        {
            if (amount <= 0)
                throw new NegativeException(amount, product);

            if (product.Qty <= 0 || product.NetQty <= 0)
                throw new InsufficientQuantityException(product.Qty, product.NetQty, product);

            var transactions = await _transactionsDataService.GetAll();

            var matching = transactions
                .Where(t => t.Item.Id == product.Id)
                .Reverse()
                .Take(amount)
                .ToList();

            foreach (var transaction in matching)
            {
                await _transactionsDataService.Delete(transaction.Id);
            }


            return product;
        }
    }
}
