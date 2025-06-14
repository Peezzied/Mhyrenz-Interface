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
        private readonly IProductService _productService;
        private readonly ITransactionsService _transactionService;
        private readonly TrackerManager<TransactionDataViewModel> _trackerManager;

        public TransactionStore(
            UndoRedoManager undoRedoManager,
            IEnumerable<Product> products,
            IViewModelFactory<TransactionDataViewModel> productsViewModelFactory,
            IProductService productService,
            ITransactionsService transactionsService)
        {
            _undoRedoManager = undoRedoManager;
            _transactionsViewModelFactory = productsViewModelFactory;
            _productService = productService;
            _transactionService = transactionsService;
            _trackerManager = new TrackerManager<TransactionDataViewModel>();

            
        }

        public ObservableCollection<TransactionDataViewModel> Transactions { get; } = new ObservableCollection<TransactionDataViewModel>();

        public void LoadTransactions(IEnumerable<Transaction> transactions)
        {
            Transactions.Clear();
            ChangeTracking.IsInventoryLoaded = true;

            foreach (var transaction in transactions)
            {
                var viewModel = _transactionsViewModelFactory.CreateViewModel(new TransactionDataViewModelDTO() 
                { 
                    Transaction = transaction,
                });

                //TrackTransactions(viewModel);

                Transactions.Add(viewModel);
            }
        }

        private void TrackTransactions(TransactionDataViewModel viewModel)
        {
            throw new NotImplementedException();
        }
    }
}
