using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Controls;
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
    public interface InventoryGridHost
    {
        void RowIntoView(IEnumerable<ProductDataViewModel> products);
    }

    public class InventoryViewModel : NavigationViewModel
    {
        private readonly CreateViewModel<InventoryDataGridViewModel> _inventoryDataGridViewModelFactory;
        private readonly CreateViewModel<AddProductViewModel> _addProductViewModelFactory;
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

        public ICommand AddProductCommand { get; set; }
        public ICommand ExportInventoryCommand { get; set; }

        private string _searchBar = string.Empty;
        public string SearchBar
        {
            get => _searchBar;
            set
            {
                _searchBar = value;
                OnPropertyChanged(nameof(SearchBar));
                ((InventoryTabItem)SelectedItem).Refresh(); // FIXME: OFTEN THROWS AN EXCEPTION WHEN CATEGORIES IS EMPTY

            }
        }

        private readonly HashSet<int> _initializedTabs = new HashSet<int>();

        private object _selectedItem;
        /// <summary>
        /// Selected tab.
        /// </summary>
        public object SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem == value) return;

                _selectedItem.CastTo<InventoryTabItem>()?.Dispose();
                _selectedItem = value;

                var tabItem = SelectedItem.CastTo<InventoryTabItem>();

                if (!_initializedTabs.Contains(tabItem.Id))
                {
                    // First time this tab is opened — create its VM now
                    var vm = _inventoryDataGridViewModelFactory(this, InventoryDataGridLayout.Detailed);
                    vm.SelectedItemsChanged += Vm_SelectedItemsChanged;
                    tabItem.SetViewModel(vm);
                    _initializedTabs.Add(tabItem.Id);
                }

                tabItem.ContentViewModel.Load();
                _mainViewModel.RibbonBarViewModel = tabItem;

                if (!SearchBar.IsNullOrEmpty()) tabItem.Refresh();
                OnPropertyChanged(nameof(SelectedItem));

                CanDelete = tabItem.ContentViewModel.SelectedItems?.Any() == true;
            }
        }

        private bool _canDelete = false;
        public bool CanDelete
        {
            get => _canDelete;
            set
            {
                _canDelete = value;
                OnPropertyChanged(nameof(CanDelete));
            }
        }

        public bool DrawerIsOpen
        {
            get => _drawerIsOpen;
            set
            {
                _drawerIsOpen = value;

                if (!_drawerIsOpen)
                    DrawerViewModel.Dispose();

                OnPropertyChanged(nameof(DrawerIsOpen));
            }
        }

        public AddProductDrawer DrawerContent
        {
            get => _drawerContent;
            private set
            {
                _drawerContent = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<InventoryTabItem> TabItems { get; private set; } = new ObservableCollection<InventoryTabItem>();
        public ICommand DeleteProductCommand { get; set; }

        private readonly DeleteCommand _deleteCommand;
        private bool _drawerIsOpen = false;
        private AddProductViewModel DrawerViewModel;
        private AddProductDrawer _drawerContent;
        private Drawer DrawerInstance;
        private bool IsSwitchReady = false;
        private ProductDataViewModel AddedProduct;

        public InventoryViewModel(INavigationServiceEx navigationServiceEx,
            ICategoryStore categoryStore,
            IInventoryStore inventoryStore,
            ISessionStore sessionStore,
            IProductService productService,
            IReportService reportService,
            IUndoRedoManager undoRedoManager,
            ShellViewModel shellViewModel,
            InventorySettingsProvider inventorySettingsProvider,
            AppSettingsManager appSettingsManager,
            CreateViewModel<InventoryTabItem> inventoryTabItemFactory,
            CreateViewModel<InventoryDataGridViewModel> inventoryDataGridviewModelFactory,
            CreateViewModel<AddProductViewModel> addProductViewModelFactory) : base(navigationServiceEx)
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
            _productService = productService;
            _reportService = reportService;
            _undoRedoManager = undoRedoManager;

            //_categorystore.Updated += CategoryStore_Updated;

            AddProductCommand = new RelayCommand(ShowProductAdd);
            DeleteProductCommand = new RelayCommand(DeleteCommand);
            _deleteCommand = new DeleteCommand(_productService, _inventoryStore, _undoRedoManager);
            ExportInventoryCommand = new AsyncRelayCommand(ExportCommand);

            LoadTabItems();
        }

        #region "Lifecycle and instantiation"
        public override void Dispose()
        {
            DrawerViewModel?.Dispose();

            if (DrawerInstance != null)
                DrawerInstance.Closed -= DrawerInstance_Closed;

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

        public void RowIntoView(int category, int[] products)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var tabItems = TabItems.ToDictionary(t => t.Id, t => t);

                InventoryTabItem newTab = tabItems[category];
                var vm = newTab.ContentViewModel;
                bool isDiff = false;
                if (newTab != SelectedItem)
                {
                    SelectedItem = newTab;
                    isDiff = true;
                }

                if (SearchBar != string.Empty)
                    SearchBar = string.Empty;

                var changedProductInfo = _inventoryStore.LastProductChanged.ChangedProductInfo;
                var selectIndex = changedProductInfo != null ? changedProductInfo.Value.Index : newTab.ProductIndexOf(products.First());

                if (selectIndex < 0)
                {
                    // TODO: show alert only if the index is out of range
                    return;
                }
                vm.SelectItem(isDiff, selectIndex, products);
            }), System.Windows.Threading.DispatcherPriority.Loaded);

        }

        public void SelectTab(int categoryId)
        {
            var map = TabItems.ToDictionary(t => t.Id, t => t);
            SelectedItem = map[categoryId];
        }

        #region "Helpers"

        // FIXME Decouple the view from this viewmodel
        private void RefreshDrawerContent()
        {
            var vm = _addProductViewModelFactory();
            vm.SubmitSuccess += Vm_SubmitSuccess;
            vm.RowIntoView += Vm_RowIntoView;

            DrawerContent = new AddProductDrawer
            {
                DataContext = vm
            };

            DrawerViewModel = DrawerContent.DataContext as AddProductViewModel;

        }

        // for each category, create a tab item with a datagrid and filter for the category
        private void AddTabItem(Category category, Predicate<object> filter)
        {
            //var vm = _inventoryDataGridViewModelFactory(this, InventoryDataGridLayout.Detailed);
            //vm.SelectedItemsChanged += Vm_SelectedItemsChanged;

            var tab = _inventoryTabItemFactory(category, filter,
                (Func<ProductDataViewModel, bool>)(product =>
                    string.IsNullOrWhiteSpace(SearchBar) ||
                    product.Name?.IndexOf(SearchBar, StringComparison.InvariantCultureIgnoreCase) >= 0)
            );

            TabItems.Add(tab);
        }
        #endregion

        #region "Event handlers"
        private void CategoryStore_Updated()
        {
            // TODO update category tabs when store is updated

            //var items = TabItems.ToDictionary(i => i.Id, i => i);
            //foreach (var item in _categorystore.CategoriesFilter)
            //{
            //    if (items.ContainsKey(item.Key.Id))
            //        return;
            //    AddTabItem(item);
            //}
        }

        private void DrawerInstance_Closed(object sender, RoutedEventArgs e)
        {
            if (!IsSwitchReady)
                return;

            IsSwitchReady = false;

            RowIntoView(AddedProduct.Item.CategoryId, new[] { AddedProduct.Item.Id });
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
            DrawerIsOpen = false;
            IsSwitchReady = true;
            AddedProduct = vm;
        }
        #endregion

        #region "Command handlers"
        private async Task ExportCommand(object obj)
        {
            await Task.Run(() =>
            {
                _reportService.Export(_inventoryStore.Products.Select(p => p.Item), _sessionStore.CurrentSession, App.Current.Dispatcher);
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
            if (DrawerInstance == null)
            {
                DrawerInstance = parameter as Drawer;
                DrawerInstance.Closed += DrawerInstance_Closed;
            }

            RefreshDrawerContent();
            DrawerIsOpen = true;
            //DrawerContent.DataContext = _addProductViewModelFactory();
            //DrawerViewModel = DrawerContent.DataContext as AddProductViewModel;
        }
        #endregion

    }

}
