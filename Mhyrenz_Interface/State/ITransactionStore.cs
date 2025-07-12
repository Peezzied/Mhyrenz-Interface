using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.State
{
    public interface ITransactionStore
    {
        ObservableCollection<TransactionDataViewModel> Transactions { get; }
        event EventHandler RequestTransactionsUpdate;
        void LoadTransactions(IEnumerable<Transaction> transactions);
        Task InitializeAsync();
    }
}