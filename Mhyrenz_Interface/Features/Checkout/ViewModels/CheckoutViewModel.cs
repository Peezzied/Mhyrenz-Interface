using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Dragablz;
using GongSolutions.Wpf.DragDrop;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.Utilities;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Features.Checkout.Commands;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Store;
using ObservableCollections;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Mhyrenz_Interface.Features.Checkout.ViewModels
{
    public class CheckoutViewModel : BaseViewModel, IAsyncInitializable, IDataGridTabHost
    {

        public CheckoutViewModel(ICheckoutService checkoutService,
            ISessionStore sessionStore,
            IInventoryStore inventoryStore,
            ITransactionStore transactionStore,
            ShellViewModel shellViewModel,
            CreateViewModel<CompletedSaleViewModel> completedSaleViewModel,
            CreateViewModel<SaleTabItem> saleTabItemFactory,
            CreateViewModel<InventoryDataGridViewModel> inventoryDataGridFactory)
        {
            _shellViewModel = shellViewModel;
            shellViewModel.RibbonBarViewModel = this;

            _inventoryDataGridFactory = inventoryDataGridFactory;
            _sessionStore = sessionStore;
            _inventoryStore = inventoryStore;
            _transactionStore = transactionStore;
            _checkoutService = checkoutService;
            _saleTabItemFactory = saleTabItemFactory;

            _completedSaleViewModel = completedSaleViewModel;

            AddSaleCommand = new AsyncRelayCommand(CreateSale);

            InventoryDragHandler = new InventoryDragSource(this);

            _transactionView = _transactionStore.Store.Source.CreateView(v => v);

            InventoryDataGridViewModel = _inventoryDataGridFactory(this);

            _searchTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };

            _searchTimer.Tick += SearchDebouce;

            Transactions = _transactionView.ToNotifyCollectionChanged(
                SynchronizationContextCollectionEventDispatcher.Current);
        }

        private void SearchDebouce(object sender, EventArgs e)
        {
            _searchTimer.Stop();

            InventoryDataGridViewModel.InventoryView.AttachFilter(
                p => string.IsNullOrWhiteSpace(_searchBar) ||
                     p.Name.IndexOf(_searchBar, StringComparison.OrdinalIgnoreCase) >= 0
            );
        }

        public async Task InitializeAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var sales = await _checkoutService.GetActiveSales();

            token.ThrowIfCancellationRequested();
            _startSaleCount = await _checkoutService.GetSaleSequence();

            if (sales.Count == 0)
            {
                token.ThrowIfCancellationRequested();
                await CreateSale();
                return;
            }

            List<SaleTabItem> tabs = new List<SaleTabItem>();

            token.ThrowIfCancellationRequested();
            await UiTimeSlicer.RunAsync(
                sales,
                sale =>
                {
                    token.ThrowIfCancellationRequested();
                    tabs.Add(_saleTabItemFactory(
                        this,
                        _transactionView,
                        sale));
                }
                );
            token.ThrowIfCancellationRequested();

            SaleTabItems.Clear();

            token.ThrowIfCancellationRequested();
            SaleTabItems.AddRange(tabs);
            token.ThrowIfCancellationRequested();

            SelectedItem = SaleTabItems.FirstOrDefault();

            _isInitialized = true;
        }


        private async Task CreateSale(object arg)
        {
            await CreateSale();
        }

        public async void DropCurrentTab(SaleTabItem saleTabItem, bool asCompleted)
        {
            if (!asCompleted)
            {
                await _checkoutService.DiscardSale(saleTabItem.Sale.Id);

                foreach (var item in saleTabItem.Sale.Transactions)
                {
                    _inventoryStore.Store.TryGetValue(item.ProductId, out var product);
                    product.Purchase -= item.Amount;
                }
            }

            App.UndoRedoManager.RemoveAll(c =>
                c is ISaleBoundCommand saleCommand &&
                saleCommand.SaleId == saleTabItem.Sale.Id);

            await CreateOrIgnore();

            saleTabItem.Dispose();
            SaleTabItems.Remove(saleTabItem);

            await _sessionStore.UpdateSession();
        }

        private readonly CreateViewModel<InventoryDataGridViewModel> _inventoryDataGridFactory;
        private readonly ISessionStore _sessionStore;
        private readonly IInventoryStore _inventoryStore;
        private readonly ITransactionStore _transactionStore;
        private readonly ICheckoutService _checkoutService;
        private readonly CreateViewModel<SaleTabItem> _saleTabItemFactory;
        private readonly CreateViewModel<CompletedSaleViewModel> _completedSaleViewModel;
        private readonly ShellViewModel _shellViewModel;

        public ICommand AddSaleCommand { get; private set; }

        private SaleTabItem _selectedItem;
        private int _startSaleCount;

        public SaleTabItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (ReferenceEquals(_selectedItem, value))
                    return;

                _selectedItem?.Unload();

                _selectedItem = value;

                _selectedItem?.Load();

                OnPropertyChanged(nameof(SelectedItem));
            }
        }

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

        private bool _completedSalesIsOpen;
        public bool CompletedSalesIsOpen
        {
            get => _completedSalesIsOpen;
            set
            {
                if (_completedSalesIsOpen != value)
                {
                    _completedSalesIsOpen = value;
                    OnPropertyChanged(nameof(CompletedSalesIsOpen));

                    if (_completedSalesIsOpen)
                        CompletedSaleViewModel = _completedSaleViewModel();
                    else
                    {
                        CompletedSaleViewModel.Dispose();
                        CompletedSaleViewModel = null;
                    }
                }
            }
        }

        public InventoryDataGridViewModel InventoryDataGridViewModel { get; set; }

        private string _searchBar;
        public string SearchBar
        {
            get => _searchBar;
            set
            {
                if (_searchBar == value)
                    return;

                _searchBar = value;
                OnPropertyChanged(nameof(SearchBar));

                _searchTimer.Stop();
                _searchTimer.Start();
            }
        }


        private CompletedSaleViewModel completedSaleViewModel;

        private bool _isInitialized = false;

        public CompletedSaleViewModel CompletedSaleViewModel
        {
            get => completedSaleViewModel;
            set
            {
                completedSaleViewModel = value;
                OnPropertyChanged(nameof(CompletedSaleViewModel));
            }
        }

        public ObservableCollection<SaleTabItem> SaleTabItems { get; } = new ObservableCollection<SaleTabItem>();

        public ItemActionCallback OnTabClosing => ClosingTab;

        public InventoryDragSource InventoryDragHandler { get; }

        private readonly ISynchronizedView<TransactionDataViewModel, TransactionDataViewModel> _transactionView;
        private readonly DispatcherTimer _searchTimer;

        public NotifyCollectionChangedSynchronizedViewList<TransactionDataViewModel> Transactions { get; private set; }

        private async void ClosingTab(ItemActionCallbackArgs<TabablzControl> args)
        {
            if (args.DragablzItem.DataContext is SaleTabItem saleTabItem && ClosingPrompt(saleTabItem))
            {
                DropCurrentTab(saleTabItem, asCompleted: false);
                return;
            }

            args.Cancel();
        }

        public static bool ClosingPrompt(SaleTabItem saleTabItem)
        {
            MessageBoxResult firstPrompt = MessageBox.Show(
                "You have unsaved changes. Closing this tab will discard the current sale.",
                "Unsaved Changes",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (firstPrompt != MessageBoxResult.Yes)
                return false;

            if (saleTabItem.Sale.Transactions.Count > 0)
            {
                MessageBoxResult secondPrompt = MessageBox.Show(
                    "This action cannot be undone.  \nAre you sure you want to permanently discard this sale?",
                    "Confirm Discard Sale",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (secondPrompt != MessageBoxResult.Yes)
                    return false;
            }
            return true;
        }

        public async Task CreateOrIgnore()
        {
            if (SaleTabItems.Count == 1)
            {
                await CreateSale();
            }
        }

        private async Task CreateSale()
        {
            var sale = await _checkoutService.Create(_sessionStore.CurrentSession.Id);
            SaleTabItem item = _saleTabItemFactory(
                this,
                _transactionView,
                sale);

            SaleTabItems.Add(item);
            SelectedItem = item;
        }

        public override void Dispose()
        {
            foreach (var tab in SaleTabItems.ToList())
                tab.Dispose();

            SaleTabItems.Clear();

            CompletedSaleViewModel?.Dispose();
            CompletedSaleViewModel = null;

            if (ReferenceEquals(_shellViewModel.RibbonBarViewModel, this))
                _shellViewModel.RibbonBarViewModel = null;

            _searchTimer.Tick -= SearchDebouce;

            AddSaleCommand = null;

            InventoryDataGridViewModel?.Dispose();
            InventoryDataGridViewModel = null;
        }

        public event Action<TransactionVMRowInfo> RowIntoViewRequested;

        public void RowIntoView(IRowInfo rowInfo)
        {
            var info = (TransactionVMRowInfo)rowInfo;

            RowIntoViewRequested?.Invoke(new TransactionVMRowInfo
            {
                Sale = info.Sale,
                Transactions = info.Transactions
            });
        }

        internal void SelectTab(int sale)
        {
            SelectedItem = SaleTabItems.FirstOrDefault(s => s.Sale.Id == sale);
        }

        public class InventoryDragSource : DefaultDragHandler
        {
            public InventoryDragSource(CheckoutViewModel checkoutViewModel)
            {
                CheckoutViewModel = checkoutViewModel;
            }

            public CheckoutViewModel CheckoutViewModel { get; }

            public override bool CanStartDrag(IDragInfo dragInfo)
            {
                if (dragInfo.SourceItem is ProductDataViewModel product)
                    return product.NetQty > 0;
                return false;
            }

            public override void StartDrag(IDragInfo dragInfo)
            {
                if (dragInfo.SourceItem is ProductDataViewModel product)
                {
                    CheckoutViewModel.ProductRowDetailsVisibilityMode =
                        DataGridRowDetailsVisibilityMode.Collapsed;

                    dragInfo.Data = product;
                    dragInfo.Effects = DragDropEffects.Copy;
                }
            }

            public override void DragDropOperationFinished(
                DragDropEffects operationResult,
                IDragInfo dragInfo)
            {
                CheckoutViewModel.ProductRowDetailsVisibilityMode =
                    DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
            }
        }
    }
}



