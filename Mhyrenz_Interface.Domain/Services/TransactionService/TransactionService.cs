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
            if (amount < 0)
                throw new NegativeException(amount, product);

            if (product.Qty <= 0 || product.NetQty <= 0)
                throw new InsufficientQuantityException(product.Qty, product.NetQty, product);

            var lastItem = await _transactionsDataService.GetLast();
            var isNew = lastItem != null && lastItem?.ProductId == product.Id;

            for (int i = 0; i < amount; i++)
            {
                await _transactionsDataService.Create(new Transaction
                {
                    Item = product,
                    UniqueId = isNew ? lastItem.UniqueId : default,
                });
            }

            return product;
        }

        public async Task<Product> Remove(Product product, int amount = 1)
        {
            if (amount <= 0)
                throw new NegativeException(amount, product);

            if (product.Qty <= 0 || product.NetQty <= 0)
                throw new InsufficientQuantityException(product.Qty, product.NetQty, product);

            for (int i = 0; i < amount; i++)
            {
                await _transactionsDataService.Delete(product.Id);
            }

            return product;
        }
    }
}
