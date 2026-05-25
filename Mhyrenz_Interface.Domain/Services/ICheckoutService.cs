using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.TransactionService;

namespace Mhyrenz_Interface.Domain.Services.SalesRecordService
{
    public interface ICheckoutService
    {

        /// <summary>
        /// Adds a product to a sale.
        /// </summary>
        /// <param name="sale">The target sale where the product will be added.</param>
        /// <param name="productId">The identifier of the product to add.</param>
        /// <param name="discountInfo">Discount information applied to the product.</param>
        /// <param name="amount">The quantity to add. Defaults to <c>1</c>.</param>
        /// <remarks>
        /// Sale-aware purchasing involves adding products to a specific sale
        /// during the checkout workflow.
        ///
        /// <para>
        /// If an equivalent transaction already exists in the sale
        /// (same product, discount type, and discount rate),
        /// the quantity is increased instead of creating a new transaction row.
        /// Otherwise, a new transaction is created and associated with the sale.
        /// </para>
        ///
        /// <para>
        /// The sale totals are automatically recalculated after the operation,
        /// including subtotal, total amount, and change.
        /// </para>
        ///
        /// <para>
        /// This operation persists changes to both the affected transaction
        /// and the sale in a single unit of work.
        /// </para>
        /// </remarks>
        /// <returns>
        /// A hydrated <see cref="CheckoutResult"/> containing
        /// the updated sale and the affected transaction.
        /// </returns>
        Task<CheckoutResult> AddItem(int saleId, int productId, DiscountInfo discountInfo, int amount = 1);


        /// <summary>
        /// Adds stock directly to a product without associating it with a sale.
        /// </summary>
        /// <param name="product">The target product.</param>
        /// <param name="amount">The quantity to add. Defaults to <c>1</c>.</param>
        /// <remarks>
        /// Direct purchasing refers to inventory adjustments performed
        /// outside of the checkout workflow.
        ///
        /// <para>
        /// This operation is sale-agnostic and does not require
        /// an active sale session.
        /// </para>
        ///
        /// <para>
        /// A transaction without an associated sale may be created
        /// or updated to maintain inventory movement history and auditing.
        /// </para>
        /// </remarks>
        /// <returns>
        /// The updated and hydrated <see cref="Product"/>.
        /// </returns>
        Task<Product> AddItem(int productId, Guid sessionId, int amount = 1);
        Task<Sale> CompleteSale(int saleId);
        Task<Sale> Create(Guid sessionId);
        Task DiscardSale(int saleId);
        Task<IReadOnlyList<Sale>> GetActive();
        Task<bool> HasTransactions();


        /// <summary>
        /// Subtracts quantity from an existing transaction within a sale.
        /// </summary>
        /// <param name="sale">The sale containing the transaction.</param>
        /// <param name="transactionId">The identifier of the transaction to modify.</param>
        /// <param name="amount">The quantity to subtract. Defaults to <c>1</c>.</param>
        /// <remarks>
        /// Sale-aware subtraction modifies an existing transaction
        /// during the checkout workflow.
        ///
        /// <para>
        /// If the resulting quantity reaches zero,
        /// the transaction may be removed from the sale.
        /// </para>
        ///
        /// <para>
        /// The sale totals are automatically recalculated after the operation,
        /// including subtotal, total amount, and change.
        /// </para>
        ///
        /// <para>
        /// This operation persists changes to both the affected transaction
        /// and the sale in a single unit of work.
        /// </para>
        /// </remarks>
        /// <returns>
        /// A hydrated <see cref="CheckoutResult"/> containing
        /// the updated sale and the affected transaction.
        /// </returns>
        Task<CheckoutResult> Subtract(int saleId, int transactionId, int amount = 1);


        /// <summary>
        /// Subtracts stock directly from a product without associating it with a sale.
        /// </summary>
        /// <param name="product">The target product.</param>
        /// <param name="amount">The quantity to subtract. Defaults to <c>1</c>.</param>
        /// <remarks>
        /// Direct subtraction refers to inventory adjustments performed
        /// outside of the checkout workflow.
        ///
        /// <para>
        /// This operation is sale-agnostic and does not require
        /// an active sale session.
        /// </para>
        ///
        /// <para>
        /// A transaction without an associated sale may be created
        /// or updated to maintain inventory movement history and auditing.
        /// </para>
        /// </remarks>
        /// <returns>
        /// The updated and hydrated <see cref="Product"/>.
        /// </returns>
        Task<Product> Subtract(int productId, Guid sessionId, int amount = 1);
    }
}