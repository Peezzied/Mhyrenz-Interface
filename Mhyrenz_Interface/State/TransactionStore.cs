using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Mhyrenz_Interface.State
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

        public async void AddToSale(CheckoutResult result)
        {
            var checkoutResult = result;

            var transaction = checkoutResult.Transaction ?? 
                throw new ArgumentNullException(nameof(checkoutResult.Transaction), "Transaction cannot be null in CheckoutResult.");

            _inventoryStore.PurchaseProduct(transaction.ProductId, transaction.Product.Purchase);

            if (checkoutResult.WasRemoved)
            {
                var productId = transaction.ProductId;
                if (Store.TryGetValue(productId, out var vm))
                {
                    await vm.RequestFlash(SaleBoundPurchaseCommand.DTO.Type.Subtract);
                    Store.Remove(productId);
                }
                return;
            }


            if (Store.TryGetValue(transaction.ProductId, out var existingVm))
            {
                existingVm.Transaction = transaction;
                await existingVm.RequestFlash(SaleBoundPurchaseCommand.DTO.Type.Add);
            }
            else
            {
                var vm = _transactionDataViewModel(transaction);
                Store.Add(vm);

                App.Current.BeginInvoke(new Action(() => vm.RequestFlash(SaleBoundPurchaseCommand.DTO.Type.AddNew)));
            }
        }

        public async Task InitializeAsync()
        {
            var transactions = (await _checkoutService.GetActive()).SelectMany(sale => sale.Transactions)
                .Select(transaction =>_transactionDataViewModel(transaction))
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
