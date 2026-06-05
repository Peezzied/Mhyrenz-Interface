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
using Microsoft.Extensions.DependencyInjection;

namespace Mhyrenz_Interface.Store
{
    public class TransactionStore : ITransactionStore
    {
        private readonly CreateViewModel<TransactionDataViewModel> _transactionDataViewModel;
        private readonly ICheckoutService _checkoutService;
        private readonly IInventoryStore _inventoryStore;
        private readonly IInventoryStore inventoryStore;

        public SourceCollection<int, TransactionDataViewModel> Store { get; }
            = new SourceCollection<int, TransactionDataViewModel>(v => v.Transaction.ProductId);

        public TransactionStore(CreateViewModel<TransactionDataViewModel> transactionDataViewModel, ICheckoutService checkoutService, IInventoryStore inventoryStore)
        {
            _transactionDataViewModel = transactionDataViewModel;
            _checkoutService = checkoutService;
            _inventoryStore = inventoryStore;
        }

        public event EventHandler<Sale> SaleChange;

        public enum OperationType { New, Update, Remove }

        public async void AddToSale(CheckoutResult result)
        {
            var checkoutResult = result;

            var transaction = checkoutResult.Transaction ??
                throw new ArgumentNullException(nameof(checkoutResult.Transaction), "Transaction cannot be null in CheckoutResult.");

            _inventoryStore.PurchaseProduct(transaction.ProductId, transaction.Product.Purchase);
            OnSaleChange(checkoutResult.Sale);

            if (checkoutResult.WasRemoved)
            {
                var productId = transaction.ProductId;
                if (Store.TryGetValue(productId, out var vm))
                {
                    await vm.RequestFlash(OperationType.Remove);
                    Store.Remove(productId);
                }
                return;
            }


            if (!(await UpdateTransaction(transaction)))
            {
                var vm = _transactionDataViewModel(transaction);
                Store.Add(vm);

                App.Current.BeginInvoke(new Action(() => vm.RequestFlash(OperationType.New)));
            }
        }

        public async Task<bool> UpdateTransaction(Transaction transaction)
        {
            if (Store.TryGetValue(transaction.ProductId, out var existingVm))
            {
                existingVm.Transaction = transaction;
                await existingVm.RequestFlash(OperationType.Update);

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
            var transactions = (await _checkoutService.GetActive()).SelectMany(sale => sale.Transactions)
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
