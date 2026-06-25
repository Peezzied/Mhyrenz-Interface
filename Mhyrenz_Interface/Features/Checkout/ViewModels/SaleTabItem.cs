using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Features.Checkout.Commands;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Shared.Behaviors;
using Mhyrenz_Interface.Store;
using ObservableCollections;
using Setter = Mhyrenz_Interface.Core.PropertyTracking.TrackPropertyHelper.Setter;

namespace Mhyrenz_Interface.Features.Checkout.ViewModels
{
    public class SaleTabItem : ValidationViewModel, IEditCancelState, IFlashRequestable
    {
        public SaleTabItem(
            Sale sale,
            CheckoutViewModel parent,
            ITransactionStore transactionStore,
            ICheckoutService checkoutService,
            ISessionStore sessionStore,
            ISynchronizedView<TransactionDataViewModel, TransactionDataViewModel> transactionView,
            CreateViewModel<TransactionDataViewModel> transactionDataViewModel,
            CreateCommand<CheckoutCommand> checkoutCommand,
            CreateCommand<TransactionVMCommandPurchase> transctionPurchaseCommand,
            CreateCommand<TransactionVMCommandDelete> transctionDeleteCommand,
            CreateCommand<TransactionVMCommandDiscount> transactionDiscountCommand)
        {
            _transactionDataViewModel = transactionDataViewModel;
            _transactionStore = transactionStore;
            _checkoutService = checkoutService;
            _sessionStore = sessionStore;
            Owner = parent;
            _transactionView = transactionView;

            CheckoutCommand = new AsyncRelayCommand(CheckoutAction, ValidateCheckout);
            VoidCommand = new AsyncRelayCommand(VoidAction);

            Sale = sale;
            Header = sale.GetCustomerName();

            _checkoutCommand = checkoutCommand;
            _transctionPurchaseCommand = transctionPurchaseCommand;
            _transctionDeleteCommand = transctionDeleteCommand;
            _transactionDiscountCommand = transactionDiscountCommand;
            SaleDropHandler = new SaleDropTarget(this, transactionStore);

            RemoveCommand = new AsyncRelayCommand(RemoveAction);
            DiscountCommand = new AsyncRelayCommand(DiscountAction, CanDiscountCommand);
            AddCommand = new HandyControl.Tools.Command.RelayCommand(AddAction);

        }

        protected void AddAction(object obj)
        {
            var product = ((ProductDataViewModel)obj).Item;

            if (_transactionStore.Store.TryGetValue(
                    Transaction.CreateTransactionKey(product.Id, Sale.Id), out var transaction))
            {
                transaction.Qty += 1;
            }
            else
            {
                TrackQtyProps(propertyName: nameof(TransactionDataViewModel.Qty),
                    productId: product.Id,
                    oldValue: 0,
                    newValue: 1);
            }
        }

        private bool CanDiscountCommand(object obj)
        {
            return Sale.Discount == Domain.Models.Discount.None || Sale.Discount == (Discount)obj || (Discount)obj == Domain.Models.Discount.None;
        }

        private async Task RemoveAction(object obj)
        {
            await App.UndoRedoManager.Execute(_transctionDeleteCommand(new TransactionVMCommandDelete.DTO
            {
                SaleId = Sale.Id,
                Transactions = !(obj is TransactionDataViewModel transaction)
                    ? SelectedItems.Select(t => t.Transaction.Id).ToList()
                    : new[] { transaction.Transaction.Id }.ToList()
            }));
        }

        private async Task DiscountAction(object obj)
        {
            await App.UndoRedoManager.Execute(_transactionDiscountCommand(new TransactionVMCommandDiscount.DTO
            {
                Discount = (Discount)obj,
                SaleId = Sale.Id,
                Transactions = SelectedItems.Select(t => t.Transaction).ToList()
            }));
        }

        private readonly ISynchronizedView<TransactionDataViewModel, TransactionDataViewModel> _transactionView;

        private bool _isLoaded;
        private bool _disposed;

        private async Task VoidAction(object arg)
        {
            if (CheckoutViewModel.ClosingPrompt(this))
                Owner.DropCurrentTab(this, asCompleted: false);
        }

        private async Task CheckoutAction(object obj)
        {
            var result = MessageBox.Show(
                "Do you want to complete this sale?",
                "Complete Sale",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            var sale = await _checkoutService.CompleteSale(Sale.Id, _received);

            _transactionStore.OnSaleChange(sale);
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

                Header = Sale.GetCustomerName();
            }
        }

        private string _header;
        public string Header
        {
            get => _header;
            set
            {
                if (_header != value)
                {
                    _header = value;
                    OnPropertyChanged(nameof(Header));
                }
            }
        }

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
        public decimal Discount => Sale.SubTotal - Sale.Total;
        public bool HasDiscount => Discount > 0;

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
        private readonly CreateCommand<TransactionVMCommandDelete> _transctionDeleteCommand;
        private readonly CreateCommand<TransactionVMCommandDiscount> _transactionDiscountCommand;

        public SaleDropTarget SaleDropHandler { get; }
        public AsyncRelayCommand RemoveCommand { get; }
        public AsyncRelayCommand CheckoutCommand { get; private set; }
        public AsyncRelayCommand VoidCommand { get; private set; }

        private readonly CreateViewModel<TransactionDataViewModel> _transactionDataViewModel;
        private readonly ITransactionStore _transactionStore;
        private readonly ICheckoutService _checkoutService;
        private readonly ISessionStore _sessionStore;

        public event EventHandler<RowFlashRequestedEventArgs> FlashRequested;

        public CheckoutViewModel Owner { get; private set; }

        public bool IsEditCancelled { get; set; }
        public AsyncRelayCommand DiscountCommand { get; }
        public HandyControl.Tools.Command.RelayCommand AddCommand { get; }

        public void Load()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SaleTabItem));

            if (_isLoaded)
                return;

            _transactionView.AttachFilter(TransactionFilter);

            _transactionView.ViewChanged += View_ViewChanged;

            foreach (var (_, view) in _transactionView.Filtered)
                view.TrackedPropertyChanged += Transaction_TrackedPropertyChanged;

            _transactionStore.SaleChange += TransactionStore_SaleChange;

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
                    Owner.DropCurrentTab(this, asCompleted: true);
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
            TrackQtyProps(args.PropertyName, viewModel.Transaction.ProductId, args.OldValue, args.NewValue, viewModel.Transaction.Id);
        }

        public TrackPropertyHelper<long, TransactionDataViewModel> TrackQtyProps(string propertyName, int productId, object oldValue, object newValue, int? transactionId = null)
        {
            var tracker = TrackPropertyHelper.Build(_transactionStore, Transaction.CreateTransactionKey(productId, Sale.Id), propertyName)
                .Track(nameof(TransactionDataViewModel.QtyIncrementEdit), (setter, key) =>
                {
                    oldValue = 0;
                    method(setter, key);
                })
                .Track(nameof(TransactionDataViewModel.Qty), method);

            void method(Setter setter, long key)
            {
                App.UndoRedoManager.Execute(_transctionPurchaseCommand(new TransactionVMCommandPurchase.DTO
                {
                    SaleId = Sale.Id,
                    ProductId = productId,
                    TransactionId = transactionId ?? 0,
                    ChangedArgs = new PropertyChangeCommand<TransactionVMRowInfo>.ChangedArgs
                    {
                        OldValue = oldValue,
                        NewValue = newValue,
                        RowInfo = new TransactionVMRowInfo
                        {
                            Sale = Sale.Id
                        }
                    },
                    Setter = setter
                }));
            }

            return tracker;
        }

        protected override IRaiseCanExecuteChanged SubmitActionCommand()
        {
            return CheckoutCommand;
        }

        public async Task RequestFlash(IFlashReceiver item, DataGridFlashBehavior.OperationType type)
        {
            await FlashRequested.RequestFlash(item, type);
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
                saleTabItem.AddAction((ProductDataViewModel)dropInfo.Data);
            }
        }
    }
}