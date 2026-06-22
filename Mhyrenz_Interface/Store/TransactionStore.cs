using System;
using System.Linq;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Core.Collection;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Features.Checkout.ViewModels;
using Mhyrenz_Interface.Shared.Behaviors;
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

        public async void AddToSale(CheckoutResult result, int amount)
        {
            var transaction = result.Transaction ??
                throw new ArgumentNullException(nameof(result.Transaction), "Transaction cannot be null in CheckoutResult.");

            _inventoryStore.PurchaseProduct(transaction.ProductId, transaction.IsDeleted ? -amount : amount);

            if (result.Sale != null)
                OnSaleChange(result.Sale);

            if (transaction.IsDeleted)
            {
                if (Store.TryGetValue(transaction.TransactionKey, out var vm))
                {
                    await vm.RequestFlash(DataGridFlashBehavior.OperationType.Remove);
                    Store.Remove(transaction.TransactionKey);
                }
                return;
            }

            if (!(await UpdateTransaction(transaction)))
            {
                var vm = _transactionDataViewModel(transaction);
                Store.Add(vm);

                App.Current.BeginInvoke(new Action(() => vm.RequestFlash(DataGridFlashBehavior.OperationType.New)));
            }
        }

        public async Task RemoveFromSale(CheckoutResult result)
        {
            var isDeleted = false;

            OnSaleChange(result.Sale);

            async Task apply(Transaction transaction)
            {
                _inventoryStore.Store.TryGetValue(transaction.ProductId, out var productVm);
                Store.TryGetValue(transaction.TransactionKey, out var transactionVm);


                if (transaction.IsDeleted)
                {
                    //await transactionVm.RequestFlash(DataGridFlashBehavior.OperationType.Remove);
                    productVm.Purchase -= transaction.Amount;
                }   
                else
                    productVm.Purchase += transaction.Amount;
                isDeleted = transaction.IsDeleted;
            }

            await Task.WhenAll(result.Transactions.Select(t => apply(t)));


            if (isDeleted)
            {
                Store.RemoveMany(result.Transactions.Select(t => t.TransactionKey));
                return;
            }

            var vms = result.Transactions.Select(t => _transactionDataViewModel(t));
            Store.AddRange(vms);

            //_ = App.Current.Dispatcher.BeginInvoke(new Action(async () => await Task.WhenAll(vms.Select(t => t.RequestFlash(DataGridFlashBehavior.OperationType.New)))));
        }

        public async Task<bool> UpdateTransaction(Transaction transaction)
        {
            if (Store.TryGetValue(transaction.TransactionKey, out var existingVm))
            {
                existingVm.Transaction = transaction;
                await existingVm.RequestFlash(DataGridFlashBehavior.OperationType.Update);

                return true;
            }
            return false;
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
