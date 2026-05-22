using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using Mhyrenz_Interface.Domain.Exceptions;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;

namespace Mhyrenz_Interface.Domain.Services.TransactionService
{
    public class TransactionService : ITransactionsService
    {
        private readonly ITransactionsDataService _transactionsDataService;
        private readonly ICheckoutService _salesService;

        public TransactionService(
            ITransactionsDataService transactionsDataService,
            ICheckoutService salesService)
        {
            _transactionsDataService = transactionsDataService;
            _salesService = salesService;
        }

        public async Task<Transaction> Add(Product product, Sale sale, DiscountInfo discountInfo, int amount = 1)
        {
            return await Create(new Transaction
            {
                ProductId = product.Id,
                SaleId = sale.Id,
                Amount = amount,
                RetailPrice = product.RetailPrice,
                Discount = discountInfo.Discount,
                DiscountRate = discountInfo.DiscountRate
            });
        }

        public async Task<Transaction> Add(Transaction transaction, int amount = 1)
        {
            transaction.Amount += amount;
            return await Update(transaction);
        }

        public async Task<Transaction> Add(Product product, int amount = 1)
        {
            var transaction = await _transactionsDataService.GetByProductId(product.Id);
            if (transaction == null)
            {
                return await Create(new Transaction
                {
                    ProductId = product.Id,
                    Amount = amount,
                    RetailPrice = product.RetailPrice
                });
            }

            return await Add(transaction, amount);
        }

        public async Task Clear()
        {
            var transactions = await _transactionsDataService.GetAll();

            await _transactionsDataService.DeleteMany(transactions);

            await _transactionsDataService.Clean();
        }

        public async Task<Transaction> Create(Transaction transaction)
        {
            transaction = await _transactionsDataService.Create(transaction);
            transaction.Sale = await _salesService.Update(transaction.Sale);
            return transaction;
        }

        public async Task<Transaction> Subtract(Product product, int amount = 1)
        {
            var transaction = await _transactionsDataService.GetByProductId(product.Id);
            if (transaction == null)
            {
                return await Create(new Transaction
                {
                    ProductId = product.Id,
                    Amount = -amount,
                    RetailPrice = product.RetailPrice
                });
            }

            return await Subtract(transaction, amount);
        }

        public async Task<Transaction> Subtract(Transaction transaction, int amount = 1)
        {
            transaction.Amount -= amount;
            return await Update(transaction);
        }

        public Task<Transaction> Update(Transaction transaction)
        {
            transaction.RetailPrice = transaction.Item.RetailPrice;
            return _transactionsDataService.Update(transaction);
        }

        public async Task<IEnumerable<Transaction>> UpdateRange(IEnumerable<Transaction> transactions)
        {
            return await _transactionsDataService.UpdateMany(transactions);
        }
    }
}
