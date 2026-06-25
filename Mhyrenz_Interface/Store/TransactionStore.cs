using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core.Collection;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Features.Checkout.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Mhyrenz_Interface.Store
{
    public class TransactionStore : ITransactionStore
    {
        private readonly CreateViewModel<TransactionDataViewModel> _transactionDataViewModel;
        private readonly ICheckoutService _checkoutService;
        private readonly IInventoryStore _inventoryStore;
        private readonly IInventoryStore inventoryStore;

        public SourceCollection<long, TransactionDataViewModel> Store { get; }
            = new SourceCollection<long, TransactionDataViewModel>(v => v.Transaction.TransactionKey);

        public TransactionStore(CreateViewModel<TransactionDataViewModel> transactionDataViewModel, ICheckoutService checkoutService, IInventoryStore inventoryStore)
        {
            _transactionDataViewModel = transactionDataViewModel;
            _checkoutService = checkoutService;
            _inventoryStore = inventoryStore;
        }

        public event EventHandler<Sale> SaleChange;

        public TransactionDataViewModel AddTransaction(Transaction transaction)
        {
            var vm = _transactionDataViewModel(transaction);
            Store.Add(vm);

            return vm;
        }

        public IEnumerable<TransactionDataViewModel> AddManyTransactions(IEnumerable<Transaction> transactions)
        {
            var vms = transactions.Select(t => _transactionDataViewModel(t));
            Store.AddRange(vms);

            return vms;
        }

        public void OnSaleChange(Sale sale)
        {
            SaleChange?.Invoke(this, sale);
        }

        public async Task InitializeAsync()
        {
            var transactions = (await _checkoutService.GetAllTransactions())
                .Select(transaction => _transactionDataViewModel(transaction))
                .ToList();

            Store.AddRange(transactions);
        }

        public static async Task LoadTransactionStore(IServiceProvider sp)
        {
            var transactionStore = sp.GetRequiredService<ITransactionStore>();
            await transactionStore.InitializeAsync();
        }

    }
}
