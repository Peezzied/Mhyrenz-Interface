using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        private List<PropertyChangeTracker<TransactionDataViewModel>> _trackers = new List<PropertyChangeTracker<TransactionDataViewModel>>();

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

            _inventoryStore.PurchaseEvent += async (s, e) => await InitializeAsync();
            _inventoryStore.AddProductEvent += async (s, e) => await InitializeAsync();
            _inventoryStore.RemoveProductEvent += async (s, e) => await InitializeAsync();
        }


        private ObservableCollection<ProductDataViewModel> _products => _inventoryStore.Products;

        public ObservableCollection<TransactionDataViewModel> Transactions { get; } = new ObservableCollection<TransactionDataViewModel>();

        public async Task InitializeAsync()
        {
            var products = await _transactionService.GetLatests();
            LoadTransactions(products);
        }

        public void LoadTransactions(IEnumerable<Transaction> transactions)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _trackers.Clear();
                Transactions.Clear();

                if (transactions == null)
                    return;

                var productById = _products.ToDictionary(
                       p => p.Item.Id,
                       p => p
                   ); // TODO: use the lookup table in the inventorystore instead when it's implemented

                var displayTransactions = transactions
                    .GroupBy(t => t.UniqueId)
                    .Select(group =>
                    {
                        var first = group.First();

                        productById.TryGetValue(first.ProductId, out var product);

                        return _transactionsViewModelFactory(new TransactionDataViewModelDTO()
                        {
                            Id = group.Key,
                            Product = product,
                            Amount = group.Count(),
                            Date = group.Max(t => t.Timestamp),
                            Session = first.Session,
                        });
                    });

                foreach (var item in displayTransactions)
                {
                    if (!item.Product.Item.IsDeleted)
                        _trackers.Add(TrackTransactions(item));

                    Transactions.Add(item);
                }
            });


        }

        private PropertyChangeTracker<TransactionDataViewModel> TrackTransactions(TransactionDataViewModel viewModel)
        {
            void method(PropertyChangeTracker<TransactionDataViewModel> tracker, TargetChangedEventArgs args, object oldValue, object newValue)
            {
                HandleBarcodeChange(args);
            }

            var _tracker = new PropertyChangeTracker<TransactionDataViewModel>(viewModel);

            _tracker
                .Track(nameof(TransactionDataViewModel.Barcode), viewModel.Barcode, method);

            return _tracker;
        }

        private void HandleBarcodeChange(TargetChangedEventArgs args)
        {
            //var target = (TransactionDataViewModel)args.Target;
            //var propertyName = args.PropertyOf;

            //var productId = target.Product.Item.Id;

            //var productLookup = _products.ToDictionary(p => p.Item.Id);

            //foreach (var transaction in Transactions)
            //{
            //    if (productLookup.TryGetValue(productId, out var product) && transaction.Product.Item.Id == productId)
            //    {
            //        transaction.Product = product;
            //    }
            //}
        }

        public static async Task LoadTransactionStore(IServiceProvider serviceProvider)
        {
            var store = serviceProvider.GetRequiredService<ITransactionStore>();
            await store.InitializeAsync();
        }
    }
}
