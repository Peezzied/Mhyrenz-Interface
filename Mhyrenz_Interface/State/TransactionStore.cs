using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.State
{
    public class TransactionStore : ITransactionStore
    {
        private readonly UndoRedoManager _undoRedoManager;
        private readonly IViewModelFactory<TransactionDataViewModel> _transactionsViewModelFactory;
        private readonly IInventroyStore _inventoryStore;
        private readonly ITransactionsService _transactionService;
        private readonly TrackerManager<TransactionDataViewModel> _trackerManager;

        public TransactionStore(
            UndoRedoManager undoRedoManager,
            IEnumerable<Product> products,
            IViewModelFactory<TransactionDataViewModel> productsViewModelFactory,
            IInventroyStore inventoryStore,
            ITransactionsService transactionsService)
        {
            _undoRedoManager = undoRedoManager;
            _transactionsViewModelFactory = productsViewModelFactory;
            _inventoryStore = inventoryStore;
            _transactionService = transactionsService;
            _trackerManager = new TrackerManager<TransactionDataViewModel>();

            
        }

        private ObservableCollection<ProductDataViewModel> _products => _inventoryStore.Products;

        public ObservableCollection<TransactionDataViewModel> Transactions { get; } = new ObservableCollection<TransactionDataViewModel>();

        public void LoadTransactions(IEnumerable<Transaction> transactions)
        {
            Transactions.Clear();
            ChangeTracking.IsInventoryLoaded = true;

            var displayTransaction = transactions
                .GroupBy(transaction => transaction.UniqueId)
                .Select(group => _transactionsViewModelFactory.CreateViewModel(new TransactionDataViewModelDTO() { 
                    Product = _products.FirstOrDefault(p => p.Item.Id == group.First().ProductId),
                    Amount = group.Count(),
                    Date = group.Max(t => t.CreatedAt),
                }));

            foreach (var item in displayTransaction)
            {
                Transactions.Add(item);
            }


        }

        private void TrackTransactions(TransactionDataViewModel viewModel)
        {
            throw new NotImplementedException();
        }
    }
}
