using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GongSolutions.Wpf.DragDrop;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.Utilities;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.Services.ReportsService;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Features.Inventory.Commands;
using Mhyrenz_Interface.Features.Orders.ViewModels;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Store;
using ObservableCollections;

namespace Mhyrenz_Interface.Features.Inventory.ViewModels
{
    public interface IRowInfo { }

    public interface IDataGridTabHost
    {
        void RowIntoView(IRowInfo rowInfo);
    }

    public class InventoryViewModel : BaseViewModel, IDataGridTabHost, IAsyncInitializable
    {

        private readonly CreateViewModel<InventoryDataGridViewModel> _inventoryDataGridViewModelFactory;
        private readonly CreateViewModel<AddProductViewModel> _addProductViewModelFactory;
        private readonly CreateViewModel<PlaceOrderViewModel> _placeOrderViewModelFactory;
        private readonly ShellViewModel _mainViewModel;
        private readonly ICategoryStore _categorystore;
        private readonly IInventoryStore _inventoryStore;
        private readonly ISessionStore _sessionStore;
        private readonly CreateViewModel<InventoryTabItem> _inventoryTabItemFactory;
        private readonly IProductService _productService;
        private readonly IReportService _reportService;
        private readonly ITransactionStore _transactionStore;
        private readonly ICheckoutService _checkoutService;
        private readonly IUndoRedoManager _undoRedoManager;
        private readonly IOrderStore _orderStore;

        private string _searchBar = string.Empty;
        private InventoryTabItem _selectedItem;
        private bool _addProductIsOpen = false;
        private AddProductViewModel _addProductViewModel;
        private bool _placeOrderIsOpen;
        private PlaceOrderViewModel _placeOrderViewModel;
        private bool IsSwitchReady = false;
        private ProductDataViewModel AddedProduct;

        public InventoryDataGridViewModel InventoryDataGrid { get; private set; }

        public InventoryDragSource InventoryDragHandler { get; }

        public RelayCommand PlaceOrderCommand { get; }
        public ICommand AddProductCommand { get; set; }
        public ICommand ExportInventoryCommand { get; set; }

        private readonly DispatcherTimer _searchTimer;

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

        /// <summary>
        /// Selected tab.
        /// </summary>
        public InventoryTabItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (ReferenceEquals(_selectedItem, value))
                    return;

                _selectedItem?.Unload();

                _selectedItem = value;

                if (_selectedItem != null)
                {
                    _selectedItem.ColumnsLoaded += SelectedItem_ColumnsLoaded;
                    _selectedItem.Load();
                    _mainViewModel.RibbonBarViewModel = _selectedItem;

                    if (!SearchBar.IsNullOrEmpty())
                    {
                        _searchTimer.Stop();
                        _searchTimer.Start();
                    }

                    DeleteProductCommand.OnCanExecuteChanged();
                }

                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        private void SelectedItem_ColumnsLoaded()
        {
            SelectedItem.ColumnsLoaded -= SelectedItem_ColumnsLoaded;

            if (PlaceOrderIsOpen)
                _selectedItem.PlaceOrderMode(true);
        }

        public ObservableCollection<InventoryTabItem> TabItems { get; private set; } = new ObservableCollection<InventoryTabItem>();
        public AsyncRelayCommand DeleteProductCommand { get; set; }

        private readonly CreateCommand<DeleteCommand> _deleteCommand;



        #region Drawers

        public bool AddProductIsOpen
        {
            get => _addProductIsOpen;
            set
            {
                _addProductIsOpen = value;

                if (!_addProductIsOpen) // closed
                    AddProductClosed();

                OnPropertyChanged(nameof(AddProductIsOpen));
            }
        }

        public AddProductViewModel AddProductViewModel
        {
            get => _addProductViewModel;
            set
            {
                _addProductViewModel = value;
                OnPropertyChanged(nameof(AddProductViewModel));
            }
        }

        public bool PlaceOrderIsOpen
        {
            get => _placeOrderIsOpen;
            set
            {
                _placeOrderIsOpen = value;

                if (!_placeOrderIsOpen) // closed
                    PlaceOrderClosed();

                OnPropertyChanged(nameof(PlaceOrderIsOpen));
            }
        }


        public PlaceOrderViewModel PlaceOrderViewModel
        {
            get => _placeOrderViewModel;
            set
            {
                _placeOrderViewModel = value;
                OnPropertyChanged(nameof(PlaceOrderViewModel));
            }
        }

        public HashSet<int> ProductsInActiveSales { get; private set; } = new HashSet<int>();
        public HashSet<int> ProductsInTransactions { get; private set; } = new HashSet<int>();

        #endregion


        public InventoryViewModel(
            ICategoryStore categoryStore,
            IInventoryStore inventoryStore,
            ISessionStore sessionStore,
            IProductService productService,
            IReportService reportService,
            ITransactionStore transactionStore,
            ICheckoutService checkoutService,
            IUndoRedoManager undoRedoManager,
            IOrderStore orderStore,
            ShellViewModel shellViewModel,
            CreateCommand<DeleteCommand> deleteCommand,
            CreateViewModel<InventoryTabItem> inventoryTabItemFactory,
            CreateViewModel<InventoryDataGridViewModel> inventoryDataGridviewModelFactory,
            CreateViewModel<AddProductViewModel> addProductViewModelFactory,
            CreateViewModel<PlaceOrderViewModel> placeOrderViewModelFactory)
        {
            _mainViewModel = shellViewModel;
            _categorystore = categoryStore;
            _inventoryStore = inventoryStore;
            _sessionStore = sessionStore;
            _inventoryTabItemFactory = inventoryTabItemFactory;
            _inventoryDataGridViewModelFactory = inventoryDataGridviewModelFactory;
            _addProductViewModelFactory = addProductViewModelFactory;
            _placeOrderViewModelFactory = placeOrderViewModelFactory;
            _productService = productService;
            _reportService = reportService;
            _transactionStore = transactionStore;
            _checkoutService = checkoutService;
            _undoRedoManager = undoRedoManager;
            _orderStore = orderStore;

            //_categorystore.Updated += CategoryStore_Updated;
            PlaceOrderCommand = new RelayCommand(PlaceOrderAction);
            AddProductCommand = new RelayCommand(ShowProductAdd);
            DeleteProductCommand = new AsyncRelayCommand(DeleteCommand, CanDeleteCommand);
            _deleteCommand = deleteCommand;
            ExportInventoryCommand = new AsyncRelayCommand(ExportCommand);

            _searchTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };

            _searchTimer.Tick += SearchDebouce;

            InventoryDragHandler = new InventoryDragSource(this);
        }

        private void SearchDebouce(object sender, EventArgs e)
        {
            _searchTimer.Stop();

            var filter = InventoryDataGrid.InventoryView.Filter;

            InventoryDataGrid.InventoryView.AttachFilter(
                p => (string.IsNullOrWhiteSpace(_searchBar) ||
                     p.Name.IndexOf(_searchBar, StringComparison.OrdinalIgnoreCase) >= 0)
                     && filter.IsMatch(p, p)
            );
        }

        private bool CanDeleteCommand(object obj)
        {
            return InventoryDataGrid.SelectedItems?.All(p => !ProductsInTransactions.Contains(p.Item.Id)) ?? false;
        }


        #region Lifecycle

        public async Task InitializeAsync(CancellationToken token)
        {
            List<InventoryTabItem> tabs = new List<InventoryTabItem>();

            token.ThrowIfCancellationRequested();
            foreach (var transactions in _transactionStore.Store)
            {
                token.ThrowIfCancellationRequested();
                if (transactions.IsActive)
                    ProductsInActiveSales.Add(transactions.Transaction.ProductId);
                ProductsInTransactions.Add(transactions.Transaction.ProductId);
            }

            token.ThrowIfCancellationRequested();
            InventoryDataGrid = _inventoryDataGridViewModelFactory(this);
            InventoryDataGrid.SelectedItemsChanged += Vm_SelectedItemsChanged;
            token.ThrowIfCancellationRequested();

            await UiTimeSlicer.RunAsync(
                _categorystore.CategoriesFilter,
                x =>
                {
                    token.ThrowIfCancellationRequested();

                    var item = _inventoryTabItemFactory(x.Key, x.Value);
                    item.ContentViewModel = InventoryDataGrid;

                    tabs.Add(item);
                });
            token.ThrowIfCancellationRequested();

            token.ThrowIfCancellationRequested();
            TabItems.Clear();
            TabItems.AddRange(tabs);
            token.ThrowIfCancellationRequested();

            SelectedItem = TabItems.FirstOrDefault();
        }

        public override void Dispose()
        {
            AddProductViewModel?.Dispose();
            AddProductViewModel = null;

            PlaceOrderViewModel?.Dispose();
            PlaceOrderViewModel = null;

            if (_selectedItem != null)
            {
                _selectedItem.ColumnsLoaded -= SelectedItem_ColumnsLoaded;
                _selectedItem.Unload();
                _selectedItem = null;
            }

            InventoryDataGrid.SelectedItemsChanged -= Vm_SelectedItemsChanged;

            foreach (var item in TabItems.ToList())
            {
                item.ColumnsLoaded -= SelectedItem_ColumnsLoaded;

                item.Dispose();
            }

            TabItems.Clear();

            SearchBar = string.Empty;

            _mainViewModel.RibbonBarViewModel = null;

            base.Dispose();
        }

        #endregion

        #region Public Methods

        public void RowIntoView(IRowInfo rowInfo)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var info = (ProductVMRowInfo)rowInfo;

                var tabItems = TabItems.ToDictionary(t => t.Id, t => t);

                InventoryTabItem newTab = tabItems[info.Category];

                var vm = newTab.ContentViewModel;
                bool canSelectTab = false;
                if (newTab != SelectedItem)
                {
                    SelectedItem = newTab;
                    canSelectTab = true;
                }

                vm.SelectItem(canSelectTab, info.Products);
            });

        }

        public void SelectTab(int categoryId)
        {
            var map = TabItems.ToDictionary(t => t.Id, t => t);
            SelectedItem = map[categoryId];
        }

        #endregion

        #region Event Handlers

        private void PlaceOrderClosed()
        {
            PlaceOrderViewModel.Dispose();
            PlaceOrderViewModel = null;

            SelectedItem.PlaceOrderMode(false);
        }

        private void AddProductClosed()
        {
            if (!IsSwitchReady)
                return;

            IsSwitchReady = false;

            //RowIntoView(new ProductVMRowInfo
            //{
            //    Category = AddedProduct.Item.CategoryId,
            //    Products = new[] { AddedProduct.Item.Id }
            //});

            AddProductViewModel.Dispose();
            AddProductViewModel = null;
        }

        private void Vm_SelectedItemsChanged()
        {
            DeleteProductCommand.OnCanExecuteChanged();
        }

        private void Vm_RowIntoView(ProductDataViewModel item)
        {
            //RowIntoView(new ProductVMRowInfo
            //{
            //    Category = AddedProduct.Item.CategoryId,
            //    Products = new[] { item.Item.Id }
            //});
        }

        private void Vm_SubmitSuccess(object sender, ProductDataViewModel vm)
        {
            AddProductIsOpen = false;
            IsSwitchReady = true;
            AddedProduct = vm;
        }

        #endregion

        #region Command Handlers

        private async Task ExportCommand(object obj)
        {
            await Task.Run(() =>
            {
                _reportService.Export(_inventoryStore.Store.Select(p => p.Item), _sessionStore.CurrentSession, App.Current.Dispatcher);
            });

            Growl.Info(new GrowlInfo
            {
                ShowDateTime = false,
                Message = "Sucessfully exported an inventory report."
            });
        }

        private async Task DeleteCommand(object parameter)
        {
            var vm = SelectedItem.ContentViewModel;

            await App.UndoRedoManager.Execute(_deleteCommand(
                SelectedItem.Id,
                vm.SelectedItems.Select(t => t.Item.Id).ToList()));
        }

        private void ShowProductAdd(object parameter)
        {
            AddProductViewModel = _addProductViewModelFactory();
            //AddProductViewModel.SubmitSuccess += Vm_SubmitSuccess;
            AddProductViewModel.RowIntoView += Vm_RowIntoView;

            AddProductIsOpen = true;
        }

        private void PlaceOrderAction(object obj)
        {
            PlaceOrderViewModel = _placeOrderViewModelFactory();

            PlaceOrderIsOpen = true;

            SelectedItem.PlaceOrderMode(PlaceOrderIsOpen);

            PlaceOrderViewModel.Load();
        }

        #endregion


        public class InventoryDragSource : DefaultDragHandler
        {
            private readonly InventoryViewModel _inventoryViewModel;

            public InventoryDragSource(InventoryViewModel inventoryViewModel)
            {
                _inventoryViewModel = inventoryViewModel;
            }

            public override void StartDrag(IDragInfo dragInfo)
            {
                if (dragInfo.SourceItem is ProductDataViewModel product)
                {
                    dragInfo.Data = product;
                    dragInfo.Effects = DragDropEffects.Copy;
                }
            }

            public override bool CanStartDrag(IDragInfo dragInfo)
            {
                return _inventoryViewModel.PlaceOrderIsOpen;
            }
        }
    }
}
