using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Features.Checkout.Commands;
using Mhyrenz_Interface.Features.Checkout.Views;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Shared.Behaviors;
using Mhyrenz_Interface.Store;
using ObservableCollections;
using static Mhyrenz_Interface.Core.PropertyTracking.TrackPropertyHelper;
using Setter = Mhyrenz_Interface.Core.PropertyTracking.TrackPropertyHelper.Setter;

namespace Mhyrenz_Interface.Features.Checkout.ViewModels
{
    public class SaleTabItem : ValidationViewModel<Sale>, IEditCancelState
    {
        public SaleTabItem(
            string header,
            Sale sale,
            CheckoutViewModel parent,
            IUndoRedoManager undoRedoManager,
            ITransactionStore transactionStore,
            ISynchronizedView<TransactionDataViewModel, TransactionDataViewModel> transactionView,
            CreateViewModel<TransactionDataViewModel> transactionDataViewModel,
            CreateCommand<CheckoutCommand> checkoutCommand,
            CreateCommand<TransactionVMCommandPurchase> transctionPurchaseCommand,
            CreateCommand<TransactionVMCommandDiscount> transactionPropCommand)
        {
            _transactionDataViewModel = transactionDataViewModel;
            _undoRedoManager = undoRedoManager;
            _transactionStore = transactionStore;
            _parent = parent;
            _transactionView = transactionView;

            CheckoutCommand = new RelayCommand(CheckoutAction, ValidateCheckout);
            VoidCommand = new AsyncRelayCommand(VoidAction);

            Sale = sale;
            Header = header;

            _checkoutCommand = checkoutCommand;
            _transctionPurchaseCommand = transctionPurchaseCommand;
            _transactionPropCommand = transactionPropCommand;
            SaleDropHandler = new SaleDropTarget(this, transactionStore);

            RemoveCommand = new RelayCommand(RemoveAction);
            DiscountCommand = new RelayCommand(DiscountAction);

        }

        private void RemoveAction(object obj)
        {
            throw new NotImplementedException(); // TODO execute standalone undo redo command
        }

        private void DiscountAction(object obj)
        {
            throw new NotImplementedException(); // TODO execute standalone undo redo command
        }

        private readonly ISynchronizedView<TransactionDataViewModel, TransactionDataViewModel> _transactionView;

        private bool _isLoaded;
        private bool _disposed;

        private async Task VoidAction(object arg)
        {
            if (CheckoutViewModel.ClosingPrompt(this))
                _parent.DropCurrentTab(this, asCompleted: false);
        }

        private void CheckoutAction(object obj)
        {
            _checkoutCommand(Sale.Id, Received).Execute();
        }

        private bool ValidateCheckout(object arg)
        {
            return !HasErrors && Due > 0;
        }

        private Sale _sale;
        public Sale Sale
        {
            get => _sale;
            set
            {
                _sale = value;
                OnPropertyChanged(null);
                Validate(nameof(Received), Received);
            }
        }

        public string Header { get; set; }

        private IEnumerable<TransactionDataViewModel> _selectedItems;
        public IEnumerable<TransactionDataViewModel> SelectedItems
        {
            get => _selectedItems;
            set
            {
                _selectedItems = value;
                OnPropertyChanged(nameof(SelectedItems));
            }
        }

        public decimal Change
        {
            get
            {
                var value = Received - Due;
                if (value > 0)
                    return value;

                return 0;
            }
        }
        public decimal Due => Sale.Total;
        public int Items => Sale.Transactions.Count;
        public decimal Discount => Sale.Total - Sale.SubTotal;

        private decimal _received;
        public decimal Received
        {
            get => _received;
            set
            {
                if (SetProperty(ref _received, value))
                {
                    Validate(nameof(Received), value);
                    OnPropertyChanged(nameof(Change));
                }
            }
        }

        private readonly CreateCommand<CheckoutCommand> _checkoutCommand;
        private readonly CreateCommand<TransactionVMCommandPurchase> _transctionPurchaseCommand;
        private readonly CreateCommand<TransactionVMCommandDiscount> _transactionPropCommand;

        public SaleDropTarget SaleDropHandler { get; }
        public RelayCommand RemoveCommand { get; }
        public RelayCommand CheckoutCommand { get; private set; }
        public AsyncRelayCommand VoidCommand { get; private set; }

        private readonly CreateViewModel<TransactionDataViewModel> _transactionDataViewModel;
        private readonly IUndoRedoManager _undoRedoManager;
        private readonly ITransactionStore _transactionStore;
        private readonly CheckoutViewModel _parent;

        public bool IsEditCancelled { get; set; }
        public RelayCommand DiscountCommand { get; }

        public async void Load()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SaleTabItem));

            if (_isLoaded)
                return;

            await Task.Run(() =>
            {
                _transactionView.AttachFilter(TransactionFilter);

                _transactionView.ViewChanged += View_ViewChanged;

                foreach (var (_, view) in _transactionView.Filtered)
                    view.TrackedPropertyChanged += Transaction_TrackedPropertyChanged;

                _transactionStore.SaleChange += TransactionStore_SaleChange;
            });

            _isLoaded = true;
        }

        public void Unload()
        {
            if (!_isLoaded)
                return;

            _transactionStore.SaleChange -= TransactionStore_SaleChange;

            if (_transactionView != null)
            {
                _transactionView.ViewChanged -= View_ViewChanged;

                foreach (var (_, view) in _transactionView.Filtered)
                    view.TrackedPropertyChanged -= Transaction_TrackedPropertyChanged;

                _transactionView.Dispose();
            }

            _isLoaded = false;
        }

        public override void Dispose()
        {
            if (_disposed)
                return;

            Unload();

            CheckoutCommand = null;
            VoidCommand = null;

            base.Dispose();

            _disposed = true;
        }

        protected override void ValidateCustom(string propertyName)
        {
            if (propertyName == nameof(Received))
            {
                if (Received < Due)
                    AddError(nameof(Received), "Cash is less than total.");
            }
        }

        private void TransactionStore_SaleChange(object sender, Sale e)
        {
            if (Sale.Id == e.Id)
            {
                if (e.Completed_at != null)
                {
                    _parent.DropCurrentTab(this, asCompleted: true);
                    return;
                }
                Sale = e;

                //_transactionView.AttachFilter(TransactionFilter);
            }
        }

        private void View_ViewChanged(in SynchronizedViewChangedEventArgs<TransactionDataViewModel, TransactionDataViewModel> e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                e.NewItem.View.TrackedPropertyChanged += Transaction_TrackedPropertyChanged;
            else if (e.Action == NotifyCollectionChangedAction.Remove)
                e.OldItem.View.TrackedPropertyChanged -= Transaction_TrackedPropertyChanged;
        }

        private bool TransactionFilter(TransactionDataViewModel model)
        {
            return model.Transaction.SaleId == Sale.Id;
        }

        private void Transaction_TrackedPropertyChanged(object sender, TrackedPropertyChangedEventArgs args)
        {
            if (!args.IsTrueOrigin)
                return;

            var viewModel = sender as TransactionDataViewModel;
            TrackQtyProps(args.PropertyName, viewModel.Transaction.ProductId, args.OldValue, viewModel.Transaction.Id)
                .Track(nameof(TransactionDataViewModel.Discount), discountMethod);

            void discountMethod(Setter setter, Getter getter, long key)
            {
                _undoRedoManager.Execute(_transactionPropCommand(new TransactionVMCommandDiscount.DTO
                {
                    SaleId = Sale.Id,
                    TransactionId = ((TransactionDataViewModel)sender).Transaction.Id,
                    ChangedArgs = new PropertyChangeCommand<TransactionVMRowInfo>.ChangedArgs
                    {
                        OldValue = args.OldValue,
                        NewValue = getter(),
                        RowInfo = new TransactionVMRowInfo
                        {
                            Sale = Sale.Id
                        }
                    },
                    Setter = setter,
                    CurrentViewIn = typeof(CheckoutView)
                }));
            }
        }

        public TrackPropertyHelper<long, TransactionDataViewModel> TrackQtyProps(string propertyName, int productId, object oldValue, int? transactionId = null, object newValue = null)
        {
            var tracker = TrackPropertyHelper.Build(_transactionStore, Transaction.CreateTransactionKey(productId, Sale.Id), propertyName)
                .Track(nameof(TransactionDataViewModel.QtyIncrementEdit), (setter, getter, key) =>
                {
                    oldValue = 0;
                    method(setter, getter, key);
                })
                .Track(nameof(TransactionDataViewModel.Qty), method);

            void method(Setter setter, Getter getter, long key)
            {
                _undoRedoManager.Execute(_transctionPurchaseCommand(new TransactionVMCommandPurchase.DTO
                {
                    SaleId = Sale.Id,
                    ProductId = productId,
                    TransactionId = transactionId ?? 0,
                    ChangedArgs = new PropertyChangeCommand<TransactionVMRowInfo>.ChangedArgs
                    {
                        OldValue = oldValue,
                        NewValue = newValue ?? getter(),
                        RowInfo = new TransactionVMRowInfo
                        {
                            Sale = Sale.Id
                        }
                    },
                    Setter = setter,
                    CurrentViewIn = typeof(CheckoutView)
                }));
            }

            return tracker;
        }

        protected override IRaiseCanExecuteChanged SubmitActionCommand()
        {
            return CheckoutCommand;
        }

        public class SaleDropTarget : DefaultDropHandler
        {
            private readonly SaleTabItem saleTabItem;
            private readonly ITransactionStore transactionStore;

            public SaleDropTarget(SaleTabItem saleTabItem, ITransactionStore transactionStore)
            {
                this.saleTabItem = saleTabItem;
                this.transactionStore = transactionStore;
            }

            public override void DragOver(IDropInfo dropInfo)
            {
                base.DragOver(dropInfo);
                if (dropInfo.Data is ProductDataViewModel)
                {
                    dropInfo.Effects = DragDropEffects.Copy;
                    dropInfo.DropTargetAdorner = null;
                }
            }

            public override void Drop(IDropInfo dropInfo)
            {
                var product = (dropInfo.Data as ProductDataViewModel).Item;

                if (transactionStore.Store.TryGetValue(
                    Transaction.CreateTransactionKey(product.Id, saleTabItem.Sale.Id), out var transaction))
                {
                    transaction.Qty += 1;
                }
                else
                {
                    saleTabItem.TrackQtyProps(propertyName: nameof(TransactionDataViewModel.Qty),
                        productId: product.Id,
                        oldValue: 0,
                        newValue: 1);
                }
            }
        }
    }
}