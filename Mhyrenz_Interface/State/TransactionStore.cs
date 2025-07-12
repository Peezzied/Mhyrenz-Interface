using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mhyrenz_Interface.State
{
    public class TransactionStore : ITransactionStore
    {
        private readonly UndoRedoManager _undoRedoManager;
        private readonly IViewModelFactory<TransactionDataViewModel> _transactionsViewModelFactory;
        private readonly IInventoryStore _inventoryStore;
        private readonly ITransactionsService _transactionService;
        private List<PropertyChangeTracker<TransactionDataViewModel>> _trackers = new List<PropertyChangeTracker<TransactionDataViewModel>>();

        public event EventHandler RequestTransactionsUpdate;

        public TransactionStore(
            UndoRedoManager undoRedoManager,
            IEnumerable<Product> products,
            IViewModelFactory<TransactionDataViewModel> productsViewModelFactory,
            IInventoryStore inventoryStore,
            ITransactionsService transactionsService)
        {
            _undoRedoManager = undoRedoManager;
            _transactionsViewModelFactory = productsViewModelFactory;
            _inventoryStore = inventoryStore;
            _transactionService = transactionsService;

            _inventoryStore.PurchaseEvent += OnPurchase;
        }


        private ObservableCollection<ProductDataViewModel> _products => _inventoryStore.Products;

        public ObservableCollection<TransactionDataViewModel> Transactions { get; } = new ObservableCollection<TransactionDataViewModel>();

        private void OnPurchase(object sender, InventoryStoreEventArgs e)
        {
            RequestTransactionsUpdate?.Invoke(e.Product, EventArgs.Empty);
        }

        public async Task InitializeAsync()
        {
            var products = await _transactionService.GetLatests();
            LoadTransactions(products);
        }

        public void LoadTransactions(IEnumerable<Transaction> transactions)
        {
            //_trackers.Clear();
            //Transactions.Clear();
            //ChangeTracking.IsInventoryLoaded = true;

            //var displayTransaction = transactions
            //    .GroupBy(transaction => transaction.UniqueId)
            //    .Select(group => _transactionsViewModelFactory.CreateViewModel(new TransactionDataViewModelDTO() { 
            //        Id = group.Key,
            //        Product = _products.FirstOrDefault(p => p.Item.Id == group.First().ProductId),
            //        Amount = group.Count(),
            //        Date = group.Max(t => t.CreatedAt),
            //    }));

            //foreach (var item in displayTransaction)
            //{
            //    _trackers.Add(TrackTransactions(item));
            //    Transactions.Add(item);
            //}


            Application.Current.Dispatcher.Invoke(() =>
            {
                _trackers.Clear();
                Transactions.Clear();

                if (transactions == null)
                    return;

                var displayTransaction = transactions
                    .GroupBy(transaction => transaction.UniqueId)
                    .Select(group => _transactionsViewModelFactory.CreateViewModel(new TransactionDataViewModelDTO()
                    {
                        Id = group.Key,
                        Product = _products.FirstOrDefault(p => p.Item.Id == group.First().ProductId),
                        Amount = group.Count(),
                        Date = group.Max(t => t.CreatedAt),
                        Session = group.First().Session,
                    }));

                foreach (var item in displayTransaction)
                {
                    _trackers.Add(TrackTransactions(item));
                    Transactions.Add(item);
                }
            });


        }

        private PropertyChangeTracker<TransactionDataViewModel> TrackTransactions(TransactionDataViewModel viewModel)
        {
            Action<PropertyChangeTracker<TransactionDataViewModel>, TargetChangedEventArgs, object, object> method;

            method = (tracker, args, oldValue, newValue) =>
            {
                HandleBarcodeChange(tracker, args);
            };

            var _tracker = new PropertyChangeTracker<TransactionDataViewModel>(viewModel);

            _tracker
                .Track(nameof(TransactionDataViewModel.Barcode), viewModel.Barcode, method);

            return _tracker;
        }

        private void HandleBarcodeChange(
            PropertyChangeTracker<TransactionDataViewModel> tracker,
            TargetChangedEventArgs args)
        {
            var target = (TransactionDataViewModel)args.Target;
            var propertyName = args.PropertyOf;

            var productId = target.DTO.Product.Item.Id;

            var productLookup = _products.ToDictionary(p => p.Item.Id);

            foreach (var transaction in Transactions)
            {
                if (productLookup.TryGetValue(productId, out var product) && transaction.Product.Item.Id == productId)
                {
                    transaction.Product = product;
                }
            }
        }

        public static async Task<TransactionStore> LoadTransactionStore(IServiceProvider serviceProvider)
        {
            var store = ActivatorUtilities.CreateInstance<TransactionStore>(serviceProvider);
            await store.InitializeAsync();

            return store;
        }
    }
}
