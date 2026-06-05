using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Features.Checkout.ViewModels;

namespace Mhyrenz_Interface.Store
{
    public interface ITransactionStore : IViewModelStore<long, TransactionDataViewModel>
    {
        event EventHandler<Sale> SaleChange;

        void AddToSale(CheckoutResult result);
        Task InitializeAsync();
        void OnSaleChange(Sale sale);
        Task<bool> UpdateTransaction(Transaction transaction);
    }
}