using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Controls;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.Services.ReportsService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels.Factory;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;

namespace Mhyrenz_Interface.ViewModels
{

    public class NotBlank : ValidationRule
    {
        public string ErrorContent { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is null)
            {
                return new ValidationResult(false, "Field is required.");
            }

            return ValidationResult.ValidResult;
        }
    }

    public class InventoryViewModel : NavigationViewModel, InventoryGridHost
    {
        private readonly CreateViewModel<InventoryDataGridViewModel> _inventoryDataGridViewModelFactory;
        private readonly CreateViewModel<AddProductViewModel> _addProductViewModelFactory;
        private readonly ICategoryStore _categorystore;
        private readonly IInventoryStore _inventoryStore;
        private readonly ISessionStore _sessionStore;
        private readonly IProductService _productService;
        private readonly IReportService _reportService;

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
                ((TabItems)SelectedItem).Refresh();
            }
        }

        private object _selectedItem;
        public object SelectedItem 
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;

                var tabItem = ((TabItems)SelectedItem);

                tabItem.Refresh();
                OnPropertyChanged(nameof(SelectedItem));

                var selected = tabItem.ControlInstance.Content.CastTo<InventoryDataGridViewModel>().SelectedItems;
                if (selected != null && selected.Any())
                    CanDelete = true;
                else CanDelete = false;
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

        private bool _drawerIsOpen = false;
        private AddProductViewModel DrawerViewModel;

        public bool DrawerIsOpen
        {
            get => _drawerIsOpen;
            set
            {
                _drawerIsOpen = value;

                if (!_drawerIsOpen)
                {
                    DrawerViewModel.SubmitSuccess -= Vm_SubmitSuccess;
                    DrawerViewModel.ClearValidations?.Invoke();
                }

                OnPropertyChanged(nameof(DrawerIsOpen));
            }
        }

        public ObservableCollection<TabItems> TabItems { get; private set; }
        public ICommand DeleteProductCommand { get; set; }

        public InventoryViewModel(
            INavigationServiceEx navigationServiceEx,
            ICategoryStore categoryStore,
            IInventoryStore inventoryStore,
            ISessionStore sessionStore,
            IProductService productService,
            IReportService reportService,
            CreateViewModel<InventoryDataGridViewModel> inventoryDataGridviewModelFactory,
            CreateViewModel<AddProductViewModel> addProductViewModelFactory) : base(navigationServiceEx)
        {
            _categorystore = categoryStore;
            _inventoryStore = inventoryStore;
            _sessionStore = sessionStore;
            _inventoryDataGridViewModelFactory = inventoryDataGridviewModelFactory;
            _addProductViewModelFactory = addProductViewModelFactory;
            _productService = productService;
            _reportService = reportService;

            _categorystore.Updated += CategoryStore_Updated;

            AddProductCommand = new RelayCommand(AddProduct);
            DeleteProductCommand = new RelayCommand(DeleteCommand);
            ExportInventoryCommand = new AsyncRelayCommand(ExportCommand);

            TabItems = new ObservableCollection<TabItems>();
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                LoadTabItems();
            }));
        }


        private void CategoryStore_Updated()
        {
            var items = TabItems.ToDictionary(i => i.Id, i => i);
            foreach (var item in _categorystore.Categories)
            {
                if (items.ContainsKey(item.Key.Id)) 
                    return;
                AddTabItem(item);
            }
        }

        private AddProductDrawer _drawerContent;
        private Drawer DrawerInstance;
        private bool IsSwitchReady = false;
        private ProductDataViewModel AddedProduct;

        public event EventHandler<int> AddItem;

        public AddProductDrawer DrawerContent
        {
            get => _drawerContent;
            private set
            {
                _drawerContent = value;
                OnPropertyChanged();
            }
        }

        public void RefreshDrawerContent()
        {
            var vm = _addProductViewModelFactory();
            vm.SubmitSuccess += Vm_SubmitSuccess;

            DrawerContent = new AddProductDrawer
            {
                DataContext = vm
            };

            DrawerViewModel = DrawerContent.DataContext as AddProductViewModel;

        }

        private void Vm_SubmitSuccess(object sender, ProductDataViewModel vm)
        {
            DrawerIsOpen = false;
            IsSwitchReady = true;
            AddedProduct = vm;
        }

        // DO PROPER DISPOSE
        public override void Dispose()
        {
            if (DrawerViewModel != null)
                DrawerViewModel.SubmitSuccess -= Vm_SubmitSuccess;

            if (DrawerInstance != null)
                DrawerInstance.Closed -= DrawerInstance_Closed;

            SearchBar = string.Empty;
            foreach (var item in TabItems)
            {
                var vm = item.ControlInstance.Content.CastTo<InventoryDataGridViewModel>();
                item.Dispose();
                vm.Dispose();
                vm.SelectedItemsChanged -= Vm_SelectedItemsChanged;
            }
        }

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
            var cmd = new DeleteCommand(_productService);
            var vm = SelectedItem.CastTo<TabItems>().ControlInstance.Content.CastTo<InventoryDataGridViewModel>();

            cmd.Execute(new InventoryDataGridVmDTO
            {
                ProductData = vm.SelectedItems,
                RemoveItems = vm.RemoveItems
            });
        }

        private void AddProduct(object parameter)
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

        private void DrawerInstance_Closed(object sender, RoutedEventArgs e)
        {
            if (!IsSwitchReady)
                return;

            IsSwitchReady = false;

            RowIntoView(AddedProduct);
        }

        private void LoadTabItems()
        {
            TabItems.Clear();

            foreach (var category in _categorystore.Categories)
            {
                AddTabItem(category);
            }
        }

        private void AddTabItem(KeyValuePair<Category, ICollectionView> category)
        {
            var vm = _inventoryDataGridViewModelFactory();
            vm.SelectedItemsChanged += Vm_SelectedItemsChanged;
            var tab = new TabItems(
                vm,
                category.Key,
                category.Value,
                filter => {  },
                product => string.IsNullOrWhiteSpace(SearchBar) || product.Name?.IndexOf(SearchBar, StringComparison.InvariantCultureIgnoreCase) >= 0
            );

            TabItems.Add(tab);
        }

        private void Vm_SelectedItemsChanged(bool state)
        {
            CanDelete = state;
        }

        public void RowIntoView(ProductDataViewModel product)
        {
            var tabItems = TabItems.ToDictionary(t => t.Id, t => t);
            var categoryId = product.CategoryId;

            TabItems newTab = tabItems[categoryId];
            var vm = newTab.ControlInstance.Content.CastTo<InventoryDataGridViewModel>();
            bool isDiff = false;
            if (newTab != SelectedItem)
            {
                SelectedItem = newTab;
                isDiff = true;
            }

            SearchBar = string.Empty;
            vm.SelectItem(isDiff, newTab.ProductIndexOf(product));
        }
    }

    public interface InventoryGridHost
    {
        void RowIntoView(ProductDataViewModel product);
    }

    public class TabItems : BaseViewModel
    {
        private readonly InventoryDataGridViewModel _inventoryDataGridViewModel;

        public string Name => _category;
        public int Id => _categoryId;

        private ICollectionView Inventory;

        private ContentControl _controlInstance;
        private Predicate<object> _originalFilter;

        public ContentControl ControlInstance
        {
            get
            {
                if (_controlInstance == null)
                {
                    _controlInstance = new ContentControl
                    {
                        Content = _inventoryDataGridViewModel,
                        ContentTemplate = (DataTemplate)App.Current.FindResource("DataGridDetailedLayout")
                    };
                }
                return _controlInstance;
            }
        }

        private readonly ICollectionView _allProducts;
        private readonly string _category;
        private readonly int _categoryId;
        private readonly Func<ProductDataViewModel, bool> _searchFilter;
        private readonly Action<Predicate<object>> _resetFilter;

        public TabItems(
            InventoryDataGridViewModel inventoryDataGridViewModel,
            Category category,
            ICollectionView allProducts,
            Action<Predicate<object>> resetFilter,
            Func<ProductDataViewModel, bool> searchFilter)
        {
            _category = category.Name;
            _categoryId = category.Id;
            _allProducts = allProducts;
            _searchFilter = searchFilter;
            _resetFilter = resetFilter;

            _inventoryDataGridViewModel = inventoryDataGridViewModel; // REFACTOR WITH FACTORY

            // Kick off deferred loading (non-blocking)
            DeferInventoryInitialization();
        }

        private async void DeferInventoryInitialization()
        {
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                _originalFilter = _allProducts.Filter;

                _allProducts.Filter = item =>
                {
                    if (_originalFilter != null && !_originalFilter(item))
                        return false;

                    return _searchFilter(item as ProductDataViewModel);
                };

                Inventory = _allProducts;

                _inventoryDataGridViewModel.Inventory = Inventory;
            }, System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        private bool FilterByCategory(object item)
        {
            return item is ProductDataViewModel product &&
                   product.Item.Category.Name == _category &&
                   _searchFilter(product);
        }

        public override void Dispose()
        {
            _resetFilter(_originalFilter);
        }

        public void Refresh() => Inventory?.Refresh();

        public int ProductIndexOf(ProductDataViewModel addedProduct)
        {
            var inventory = Inventory.Cast<ProductDataViewModel>();

            return inventory.IndexOf(addedProduct);
        }
    }



}
