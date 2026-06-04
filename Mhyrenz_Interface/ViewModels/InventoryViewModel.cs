using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.AppSettingsManager;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.Services.ReportsService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels.Factory;
using Microsoft.EntityFrameworkCore.Internal;

namespace Mhyrenz_Interface.ViewModels
{

    public class NotBlank : ValidationRule
    {
        public string ErrorContent { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is null || value.ToString().IsNullOrEmpty())
            {
                return new ValidationResult(false, "Field is required.");
            }

            return ValidationResult.ValidResult;
        }
    }
    public interface IInventoryGridHost
    {
        void RowIntoView(int category, int[] products);
    }

    public class InventoryViewModel : NavigationViewModel, IInventoryGridHost
    {

        private readonly CreateViewModel<InventoryDataGridViewModel> _inventoryDataGridViewModelFactory;
        private readonly CreateViewModel<AddProductViewModel> _addProductViewModelFactory;
        private readonly CreateViewModel<PlaceOrderViewModel> _placeOrderViewModelFactory;
        private readonly AppSettingsManager _appSettingsManager;
        private readonly InventorySettingsProvider _inventorySettingsProvider;
        private readonly ShellViewModel _mainViewModel;
        private readonly ICategoryStore _categorystore;
        private readonly IInventoryStore _inventoryStore;
        private readonly ISessionStore _sessionStore;
        private readonly CreateViewModel<InventoryTabItem> _inventoryTabItemFactory;
        private readonly IProductService _productService;
        private readonly IReportService _reportService;
        private readonly IUndoRedoManager _undoRedoManager;
        private readonly IOrderStore _orderStore;
        private readonly HashSet<int> _initializedTabs = new HashSet<int>();
        private readonly DeleteCommand _deleteCommand;

        private string _searchBar = string.Empty;
        private InventoryTabItem _selectedItem;
        private bool _canDelete = false;
        private bool _addProductIsOpen = false;
        private AddProductViewModel _addProductViewModel;
        private bool _placeOrderIsOpen;
        private PlaceOrderViewModel _placeOrderViewModel;
        private bool IsSwitchReady = false;
        private ProductDataViewModel AddedProduct;


        public InventoryDragSource InventoryDragHandler { get; }

        public RelayCommand PlaceOrderCommand { get; }
        public ICommand AddProductCommand { get; set; }
        public ICommand ExportInventoryCommand { get; set; }

        public string SearchBar
        {
            get => _searchBar;
            set
            {
                _searchBar = value;
                OnPropertyChanged(nameof(SearchBar));

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
                if (_selectedItem == value) return;

                _selectedItem.Dispose();
                _selectedItem = value;

                if (!_initializedTabs.Contains(SelectedItem.Id))
                {
                    // First time this tab is opened — create its VM now
                    var vm = _inventoryDataGridViewModelFactory(this);
                    vm.SelectedItemsChanged += Vm_SelectedItemsChanged;
                    SelectedItem.SetViewModel(vm);
                    _initializedTabs.Add(SelectedItem.Id);
                }

                SelectedItem.ContentViewModel.Load();
                _mainViewModel.RibbonBarViewModel = SelectedItem;

                OnPropertyChanged(nameof(SelectedItem));

                CanDelete = SelectedItem.ContentViewModel.SelectedItems?.Any() == true;
            }
        }

        public bool CanDelete
        {
            get => _canDelete;
            set
            {
                _canDelete = value;
                OnPropertyChanged(nameof(CanDelete));
            }
        }

        public ObservableCollection<InventoryTabItem> TabItems { get; private set; } = new ObservableCollection<InventoryTabItem>();
        public ICommand DeleteProductCommand { get; set; }



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

        #endregion


        public InventoryViewModel(INavigationServiceEx navigationServiceEx,
            ICategoryStore categoryStore,
            IInventoryStore inventoryStore,
            ISessionStore sessionStore,
            IProductService productService,
            IReportService reportService,
            IUndoRedoManager undoRedoManager,
            IOrderStore orderStore,
            ShellViewModel shellViewModel,
            InventorySettingsProvider inventorySettingsProvider,
            AppSettingsManager appSettingsManager,
            CreateCommand<DeleteCommand> deleteCommand,
            CreateViewModel<InventoryTabItem> inventoryTabItemFactory,
            CreateViewModel<InventoryDataGridViewModel> inventoryDataGridviewModelFactory,
            CreateViewModel<AddProductViewModel> addProductViewModelFactory,
            CreateViewModel<PlaceOrderViewModel> placeOrderViewModelFactory) : base(navigationServiceEx)
        {
            _appSettingsManager = appSettingsManager;
            _inventorySettingsProvider = inventorySettingsProvider;
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
            _undoRedoManager = undoRedoManager;
            _orderStore = orderStore;

            //_categorystore.Updated += CategoryStore_Updated;
            PlaceOrderCommand = new RelayCommand(PlaceOrderAction);
            AddProductCommand = new RelayCommand(ShowProductAdd);
            DeleteProductCommand = new RelayCommand(DeleteCommand);
            _deleteCommand = deleteCommand();
            ExportInventoryCommand = new AsyncRelayCommand(ExportCommand);

            InventoryDragHandler = new InventoryDragSource(this);


            LoadTabItems();
        }


        #region Lifecycle

        public override void Dispose()
        {
            AddProductViewModel?.Dispose();

            foreach (var item in TabItems)
            {
                var vm = item.ContentViewModel;
                if (vm != null)
                {
                    vm.Dispose();
                    vm.SelectedItemsChanged -= Vm_SelectedItemsChanged;
                }
                item.Dispose();
            }
            SearchBar = string.Empty;
        }

        private void LoadTabItems()
        {
            TabItems.Clear();

            foreach (var category in _categorystore.CategoriesFilter)
            {
                AddTabItem(category.Key, category.Value);
            }
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

                if (SearchBar != string.Empty)
                    SearchBar = string.Empty;

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

        private void Vm_SelectedItemsChanged(bool state)
        {
            CanDelete = state;
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
            var vm = SelectedItem.CastTo<InventoryTabItem>().ContentViewModel;
            _deleteCommand.Execute(vm.SelectedItems);
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
        }

        #endregion

        #region Nested Types

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

        #endregion
    }
}
