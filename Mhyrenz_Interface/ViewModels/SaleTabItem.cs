using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using GongSolutions.Wpf.DragDrop;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Controls;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.Views;
using ObservableCollections;
using static Mhyrenz_Interface.Core.TrackPropertyHelper;
using Setter = Mhyrenz_Interface.Core.TrackPropertyHelper.Setter;

namespace Mhyrenz_Interface.ViewModels
{
    public class SaleTabItem : BaseViewModel, IEditCancelState
    {

        public SaleTabItem(
            string header,
            Sale sale,
            InventoryDataGridViewModel inventoryDataGridViewModel,
            IUndoRedoManager undoRedoManager,
            ITransactionStore transactionStore,
            CreateViewModel<TransactionDataViewModel> transactionDataViewModel,
            CreateCommand<SaleBoundPurchaseCommand> saleBoundPurchaseCommand)
        {
            _transactionDataViewModel = transactionDataViewModel;
            _undoRedoManager = undoRedoManager;
            _transactionStore = transactionStore;
            Sale = sale;
            Header = header;

            InventoryDataGridViewModel = inventoryDataGridViewModel;

            _saleBoundPurchaseCommand = saleBoundPurchaseCommand;

            SaleDropHandler = new SaleDropHandler(this, transactionStore);
            InventoryDragHandler = new InventoryDragHandler(this);
        }

        private NotifyCollectionChangedSynchronizedViewList<TransactionDataViewModel> _transactions;
        public NotifyCollectionChangedSynchronizedViewList<TransactionDataViewModel> Transactions
        {
            get => _transactions;
            private set
            {
                _transactions = value;
                OnPropertyChanged(nameof(Transactions));
            }
        }

        public Sale Sale { get; private set; }

        public string Header { get; set; }

        public InventoryDataGridViewModel InventoryDataGridViewModel { get; }

        private readonly CreateCommand<SaleBoundPurchaseCommand> _saleBoundPurchaseCommand;

        public SaleDropHandler SaleDropHandler { get; }

        public InventoryDragHandler InventoryDragHandler { get; }

        private readonly CreateCommand<TransactionVMCommandQty> _updateTransactionCommand;
        private readonly CreateViewModel<TransactionDataViewModel> _transactionDataViewModel;
        private readonly IUndoRedoManager _undoRedoManager;
        private readonly ITransactionStore _transactionStore;
        private DataGridRowDetailsVisibilityMode _productRowDetailsVisibilityMode =
            DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
        public DataGridRowDetailsVisibilityMode ProductRowDetailsVisibilityMode
        {
            get => _productRowDetailsVisibilityMode;
            set
            {
                _productRowDetailsVisibilityMode = value;
                OnPropertyChanged(nameof(ProductRowDetailsVisibilityMode));
            }
        }

        public bool IsEditCancelled { get; set; }

        public void LoadTransactions()
        {
            var view = _transactionStore.Store.Source.CreateView(v => v);

            view.AttachFilter(TransactionFilter);

            view.ViewChanged += View_ViewChanged;

            Transactions = view.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

            foreach (var (Value, View) in view.Filtered)
            {
                View.TrackedPropertyChanged += Transaction_TrackedPropertyChanged;
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
            if (args.Origin == PropertyChangeOrigin.UndoRedo)
                return;

            var viewModel = sender as TransactionDataViewModel;
            NewMethod(args.PropertyName, viewModel.Transaction.ProductId, args.OldValue, viewModel.Transaction.Id);
        }

        public void NewMethod(string propertyName, int productId, object oldValue, int? transactionId = null, object newValue = null)
        {
            TrackPropertyHelper.Build(_transactionStore, productId, propertyName)
                .Track(nameof(TransactionDataViewModel.QtyIncrementEdit), (setter, getter, key) =>
                {
                    oldValue = 0;
                    method(setter, getter, key);
                })
                .Track(nameof(TransactionDataViewModel.Qty), method);

            void method(Setter setter, Getter getter, int key)
            {
                void handlePropChange()
                {
                    //_transactionStore.AddToSale(command.Result);
                }

                _undoRedoManager.Execute(new TransactionVMCommandProp(
                    saleId: Sale.Id,
                    productId: productId,
                    transactionId: transactionId,
                    args: new PropertyChangeCommand<TransactionVMRowInfo>.ChangedArgs
                    {
                        OldValue = oldValue,
                        NewValue = newValue ?? getter(),
                        RowInfo = new TransactionVMRowInfo
                        {
                            Sale = Sale.Id
                        }
                    },
                    setter: setter,
                    command: _saleBoundPurchaseCommand(),
                    propertyChangeHandler: handlePropChange,
                    currentViewIn: typeof(CheckoutView)
                ));
            }
        }
    }

    public class InventoryDragHandler : DefaultDragHandler
    {
        public InventoryDragHandler(SaleTabItem saleTabItem)
        {
            SaleTabItem = saleTabItem;
        }

        public SaleTabItem SaleTabItem { get; }

        public override void StartDrag(IDragInfo dragInfo)
        {
            if (dragInfo.SourceItem is ProductDataViewModel product)
            {
                SaleTabItem.ProductRowDetailsVisibilityMode =
                    DataGridRowDetailsVisibilityMode.Collapsed;

                dragInfo.Data = product;
                dragInfo.Effects = DragDropEffects.Copy;
            }
        }

        public override void DragDropOperationFinished(
            DragDropEffects operationResult,
            IDragInfo dragInfo)
        {
            SaleTabItem.ProductRowDetailsVisibilityMode =
                DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
        }
    }

    public class SaleDropHandler : DefaultDropHandler
    {
        private readonly SaleTabItem saleTabItem;
        private readonly ITransactionStore transactionStore;

        public SaleDropHandler(SaleTabItem saleTabItem, ITransactionStore transactionStore)
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

            if (transactionStore.Store.TryGetValue(product.Id, out var transaction))
            {
                transaction.Qty += 1;
            }
            else
            {
                saleTabItem.NewMethod(propertyName: nameof(TransactionDataViewModel.Qty), productId: product.Id,
                    oldValue: 0, newValue: 1);
            }
        }
    }
}