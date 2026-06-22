using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GongSolutions.Wpf.DragDrop;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.Utilities;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.Services.ReportsService;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Features.Inventory.Commands;
using Mhyrenz_Interface.Features.Orders.ViewModels;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Store;
using Microsoft.EntityFrameworkCore.Internal;
using ObservableCollections;

namespace Mhyrenz_Interface.Features.Inventory.ViewModels
{
    public interface IDataGridTabHost
    {
        void RowIntoView(int tab, int[] items);
    }

    public class InventoryViewModel : NavigationViewModel, IDataGridTabHost, IAsyncInitializable
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
        private InventoryDataGridViewModel _inventoryDataGridVm;

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
        public RelayCommand DeleteProductCommand { get; set; }

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

        public HashSet<int> ProductsInCheckout { get; private set; }

        #endregion


        public InventoryViewModel(INavigationServiceEx navigationServiceEx,
            ICategoryStore categoryStore,
            IInventoryStore inventoryStore,
            ISessionStore sessionStore,
            IProductService productService,
            IReportService reportService,
            ICheckoutService checkoutService,
            IUndoRedoManager undoRedoManager,
            IOrderStore orderStore,
            ShellViewModel shellViewModel,
            CreateCommand<DeleteCommand> deleteCommand,
            CreateViewModel<InventoryTabItem> inventoryTabItemFactory,
            CreateViewModel<InventoryDataGridViewModel> inventoryDataGridviewModelFactory,
            CreateViewModel<AddProductViewModel> addProductViewModelFactory,
            CreateViewModel<PlaceOrderViewModel> placeOrderViewModelFactory) : base(navigationServiceEx)
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
            _checkoutService = checkoutService;
            _undoRedoManager = undoRedoManager;
            _orderStore = orderStore;

            //_categorystore.Updated += CategoryStore_Updated;
            PlaceOrderCommand = new RelayCommand(PlaceOrderAction);
            AddProductCommand = new RelayCommand(ShowProductAdd);
            DeleteProductCommand = new RelayCommand(DeleteCommand, CanDeleteCommand);
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

            var filter = _inventoryDataGridVm.InventoryView.Filter;

            _inventoryDataGridVm.InventoryView.AttachFilter(
                p => (string.IsNullOrWhiteSpace(_searchBar) ||
                     p.Name.IndexOf(_searchBar, StringComparison.OrdinalIgnoreCase) >= 0)
                     && filter.IsMatch(p, p)
            );
        }

        private bool CanDeleteCommand(object obj)
        {
            return _inventoryDataGridVm.SelectedItems?.Any() ?? false;
        }


        #region Lifecycle

        public async Task InitializeAsync(CancellationToken token)
        {
            List<InventoryTabItem> tabs = new List<InventoryTabItem>();

            token.ThrowIfCancellationRequested();
            ProductsInCheckout = (await _checkoutService.GetActiveSales()).SelectMany(s => s.Transactions).Select(t => t.ProductId).ToHashSet();

            token.ThrowIfCancellationRequested();
            _inventoryDataGridVm = _inventoryDataGridViewModelFactory(this);
            _inventoryDataGridVm.Load();
            _inventoryDataGridVm.SelectedItemsChanged += Vm_SelectedItemsChanged;
            token.ThrowIfCancellationRequested();

            await UiTimeSlicer.RunAsync(
                _categorystore.CategoriesFilter,
                x =>
                {
                    token.ThrowIfCancellationRequested();

                    var item = _inventoryTabItemFactory(x.Key, x.Value);
                    item.ContentViewModel = _inventoryDataGridVm;

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

            _inventoryDataGridVm.SelectedItemsChanged -= Vm_SelectedItemsChanged;

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

        public void RowIntoView(int category, int[] products)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var tabItems = TabItems.ToDictionary(t => t.Id, t => t);

                InventoryTabItem newTab = tabItems[category];

                var vm = newTab.ContentViewModel;
                bool canSelectTab = false;
                if (newTab != SelectedItem)
                {
                    SelectedItem = newTab;
                    canSelectTab = true;
                }

                vm.SelectItem(canSelectTab, products);
            }), System.Windows.Threading.DispatcherPriority.Loaded);

        }

        public void SelectTab(int categoryId)
        {
            var map = TabItems.ToDictionary(t => t.Id, t => t);
            SelectedItem = map[categoryId];
        }

        #endregion

        #region Helpers

        /// <summary>
        /// for each category, create a tab item with a datagrid and filter for the category
        /// </summary>
        private void AddTabItem(Category category, Predicate<ProductDataViewModel> filter)
        {
            //var vm = _inventoryDataGridViewModelFactory(this, InventoryDataGridLayout.Detailed);
            //vm.SelectedItemsChanged += Vm_SelectedItemsChanged;

            bool searchFilter(ProductDataViewModel vm)
            {
                if (string.IsNullOrWhiteSpace(SearchBar))
                    return true;

                return vm.Name.IndexOf(SearchBar, StringComparison.InvariantCultureIgnoreCase) >= 0;
            }

            var tab = _inventoryTabItemFactory(category, filter);

            TabItems.Add(tab);
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

            RowIntoView(AddedProduct.Item.CategoryId, new[] { AddedProduct.Item.Id });

            AddProductViewModel.Dispose();
            AddProductViewModel = null;
        }

        private void Vm_SelectedItemsChanged()
        {
            DeleteProductCommand.OnCanExecuteChanged();
        }

        private void Vm_RowIntoView(ProductDataViewModel item)
        {
            RowIntoView(AddedProduct.Item.CategoryId, new[] { item.Item.Id });
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

        private void DeleteCommand(object parameter)
        {
            var tab = (InventoryTabItem)SelectedItem;
            var vm = tab.ContentViewModel;

            _undoRedoManager.Execute(_deleteCommand(
                tab.Id,
                vm.SelectedItems.Select(t => t.Item.Id)));
        }

        private void ShowProductAdd(object parameter)
        {
            AddProductViewModel = _addProductViewModelFactory();
            AddProductViewModel.SubmitSuccess += Vm_SubmitSuccess;
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
