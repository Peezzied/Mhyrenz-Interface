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
            if (value is null || value.ToString().IsNullOrEmpty())
            {
                return new ValidationResult(false, "Field is required.");
            }

            return ValidationResult.ValidResult;
        }
    }
    public interface InventoryGridHost
    {
        void RowIntoView(ProductDataViewModel product, (int tabSelect, bool canSelect) select = default);
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
        private readonly IUndoRedoManager _undoRedoManager;

        public ICommand AddProductCommand { get; set; }
        public ICommand ExportInventoryCommand { get; set; }

        private string _searchBar = string.Empty;
        private bool _neglectSearch = false;

        public string SearchBar
        {
            get => _searchBar;
            set
            {
                _searchBar = value;
                OnPropertyChanged(nameof(SearchBar));
                ((InventoryTabItem)SelectedItem).Refresh();

            }
        }

        private object _selectedItem;
        public object SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;

                var tabItem = ((InventoryTabItem)SelectedItem);

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
                    DrawerViewModel.Dispose();

                OnPropertyChanged(nameof(DrawerIsOpen));
            }
        }

        public ObservableCollection<InventoryTabItem> TabItems { get; private set; } = new ObservableCollection<InventoryTabItem>();
        public ICommand DeleteProductCommand { get; set; }

        public InventoryViewModel(INavigationServiceEx navigationServiceEx,
            ICategoryStore categoryStore,
            IInventoryStore inventoryStore,
            ISessionStore sessionStore,
            IProductService productService,
            IReportService reportService,
            IUndoRedoManager undoRedoManager,
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
            _undoRedoManager = undoRedoManager;
            _categorystore.Updated += CategoryStore_Updated;

            AddProductCommand = new RelayCommand(ShowProductAdd);
            DeleteProductCommand = new RelayCommand(DeleteCommand);
            ExportInventoryCommand = new AsyncRelayCommand(ExportCommand);

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
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
            vm.RowIntoView += Vm_RowIntoView;

            DrawerContent = new AddProductDrawer
            {
                DataContext = vm
            };

            DrawerViewModel = DrawerContent.DataContext as AddProductViewModel;

        }

        private void Vm_RowIntoView(ProductDataViewModel item)
        {
            RowIntoView(item);
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
            DrawerViewModel?.Dispose();

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
            var cmd = new DeleteCommand(_productService, _inventoryStore, _undoRedoManager);
            var vm = SelectedItem.CastTo<InventoryTabItem>().ControlInstance.Content.CastTo<InventoryDataGridViewModel>();

            cmd.Execute(new InventoryDataGridVmDTO
            {
                ProductData = vm.SelectedItems,
            });
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
            var vm = _inventoryDataGridViewModelFactory(this);
            vm.SelectedItemsChanged += Vm_SelectedItemsChanged;
            var tab = new InventoryTabItem(vm, category.Key, category.Value,
                product => string.IsNullOrWhiteSpace(SearchBar) || product.Name?.IndexOf(SearchBar, StringComparison.InvariantCultureIgnoreCase) >= 0
            );

            TabItems.Add(tab);
        }

        private void Vm_SelectedItemsChanged(bool state)
        {
            CanDelete = state;
        }

        public void RowIntoView(ProductDataViewModel product, (int tabSelect, bool canSelect) select = default)
        {
            var tabItems = TabItems.ToDictionary(t => t.Id, t => t);
            var categoryId = select.tabSelect > 0 ? select.tabSelect : product.CategoryId;

            InventoryTabItem newTab = tabItems[categoryId];
            var vm = newTab.ControlInstance.Content.CastTo<InventoryDataGridViewModel>();
            bool isDiff = false;
            if (newTab != SelectedItem)
            {
                SelectedItem = newTab;
                isDiff = true;
            }

            if (SearchBar != string.Empty)
                SearchBar = string.Empty;

            var index = newTab.ProductIndexOf(product);
            vm.SelectItem(isDiff, index, select.canSelect);
        }
    }

}
