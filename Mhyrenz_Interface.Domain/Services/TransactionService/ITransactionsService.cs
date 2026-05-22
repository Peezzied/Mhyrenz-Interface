using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.TransactionService;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface ITransactionsService
    {
        Task Clear();
        Task<Transaction> Create(Transaction transaction);
        Task<Transaction> Update(Transaction transaction);

        /// <summary>
        /// For sale aware purchasing. Creates the first transaction.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="sale"></param>
        /// <param name="discountInfo"></param>
        /// <param name="amount"></param>
        /// <remarks>
        /// Sale aware purchasing, on the other hand, involves adding a product to a specific sale or transaction. 
        /// This is typically done during the checkout process, in the transactions page.
        /// </remarks>
        /// <returns></returns>
        Task<Transaction> Add(Product product, Sale sale, DiscountInfo discountInfo, int amount = 1);

        /// <summary>
        /// For sale aware purchasing. Adds to an existing transaction.
        /// </summary>
        /// <param name="transaction"> Sale is sourced from here.</param>
        /// <inheritdoc cref="Add(Product, Sale, DiscountInfo, int)" />
        Task<Transaction> Add(Transaction transaction, int amount = 1);

        /// <summary>
        /// For direct purchasing. Creates a transaction on demand by adding to the latest transaction without a sale.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="amount"></param>
        /// <remarks>
        /// Direct purchasing refers to the process of adding a product directly from the inventory 
        /// without associating it with a specific sale or transaction. 
        /// 
        /// <para>Direct purchasing is not aware of any sale or transaction. In this case, transaction is empty and is resolved on demand by looking up the latest transaction without a sale. 
        /// If there is no such transaction, transaction is created with negative amount. If there is such transaction, the amount is subtracted from the resolved transaction.
        /// </para>
        /// </remarks>
        /// <returns></returns>
        Task<Transaction> Add(Product product, int amount = 1);

        /// <summary>
        /// For direct purchasing. Creates a transaction on demand by subtracting from the latest transaction without a sale.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="amount"></param>
        /// <remarks>
        /// Direct purchasing refers to the process of adding a product directly from the inventory 
        /// without associating it with a specific sale or transaction. 
        /// 
        /// <para>Transaction is resolved on demand by looking up the latest transaction without a sale. 
        /// If there is no such transaction, transaction is created with negative amount. If there is such transaction, the amount is subtracted from the resolved transaction.
        /// </para>
        /// </remarks>
        /// <returns></returns>
        Task<Transaction> Subtract(Product product, int amount = 1);

        /// <summary>
        /// For Sale aware purchasing. Subtracts from an existing transaction.
        /// </summary>
        /// <param name="transaction">Product is sourced from here.</param>
        /// <param name="amount"></param>
        /// <inheritdoc cref="Add(Product, Sale, DiscountInfo, int)" />
        /// <returns></returns>
        Task<Transaction> Subtract(Transaction transaction, int amount = 1);

    }
}