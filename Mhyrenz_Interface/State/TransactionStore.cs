using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Mhyrenz_Interface.State
{
    public class TransactionStore : ITransactionStore
    {
        private readonly IUndoRedoManager _undoRedoManager;
        private readonly CreateViewModel<TransactionDataViewModel> _transactionsViewModelFactory;
        private readonly IInventoryStore _inventoryStore;
        private readonly ICategoryStore _categoryStore;
        private readonly ITransactionsService _transactionService;

        public event EventHandler RequestTransactionsUpdate;

        public TransactionStore(
            IUndoRedoManager undoRedoManager,
            CreateViewModel<TransactionDataViewModel> productsViewModelFactory,
            IInventoryStore inventoryStore,
            ICategoryStore categoryStore,
            ITransactionsService transactionsService)
        {
            _undoRedoManager = undoRedoManager;
            _transactionsViewModelFactory = productsViewModelFactory;
            _inventoryStore = inventoryStore;
            _categoryStore = categoryStore;
            _transactionService = transactionsService;

            _inventoryStore.PropertyChanged += InventoryStore_PropertyChanged;

            // FIXME: optimize this by only updating transactions of the product that was updated/added/removed
            _inventoryStore.PurchaseEvent += async (s, e) => await InitializeAsync();
            _inventoryStore.AddProductEvent += async (s, e) => await InitializeAsync();
            _inventoryStore.RemoveProductEvent += async (s, e) => await InitializeAsync();
        }

        private async void InventoryStore_PropertyChanged(object sender, InventoryStoreEventArgs e)
        {
            if (e.PropertyName == nameof(ProductDataViewModel.RetailPrice))
            {
                var transactions = (await _transactionService.GetLatests()).ToList();
                var product = e.Product.Item;

                var transaction = transactions.FirstOrDefault(x => x.ProductId == product.Id);
                if (transaction != null && transaction.Sale.Completed_at == null && transaction.Item.RetailPrice != product.RetailPrice)
                {
                    transaction.Item.RetailPrice = product.RetailPrice;
                    transactions.First(x => x.Id == transaction.Id).Item.RetailPrice = product.RetailPrice;
                    await _transactionService.Update(transaction);
                }

                LoadTransactions(transactions);
            }
        }

        public ObservableCollection<TransactionDataViewModel> Transactions { get; } = new ObservableCollection<TransactionDataViewModel>();

        public async Task InitializeAsync()
        {
            var transactions = await _transactionService.GetLatests();
            LoadTransactions(transactions);
        }

        public void LoadTransactions(IEnumerable<Transaction> transactions)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Transactions.Clear();
                Transactions.AddRange(transactions.Select(x => _transactionsViewModelFactory(x)));
            });


        }

        public static async Task LoadTransactionStore(IServiceProvider serviceProvider)
        {
            var store = serviceProvider.GetRequiredService<ITransactionStore>();
            await store.InitializeAsync();
        }
    }
}
