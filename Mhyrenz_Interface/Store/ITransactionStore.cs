using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Features.Checkout.ViewModels;

namespace Mhyrenz_Interface.Store
{
    public interface ITransactionStore : IViewModelStore<long, TransactionDataViewModel>
    {
        event EventHandler<Sale> SaleChange;

        IEnumerable<TransactionDataViewModel> AddManyTransactions(IEnumerable<Transaction> transactions);
        TransactionDataViewModel AddTransaction(Transaction transaction);
        Task InitializeAsync();
        void OnSaleChange(Sale sale);
    }
}