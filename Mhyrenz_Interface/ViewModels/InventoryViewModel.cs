using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels.Factory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Mhyrenz_Interface.ViewModels
{
    public class InventoryViewModel : NavigationViewModel
    {
        private readonly ICategoryStore _categorystore;
        private readonly IInventoryStore _inventoryStore;

        private string _searchBar = string.Empty;
        public string SearchBar
        {
            get => _searchBar;
            set
            {
                _searchBar = value;
                OnPropertyChanged(nameof(SearchBar));
                
            }
        }
        public ObservableCollection<TabItems> TabItems { get; private set; }

        public InventoryViewModel(INavigationServiceEx navigationServiceEx, ICategoryStore categoryStore, IInventoryStore inventoryStore) : base(navigationServiceEx)
        {
            _categorystore = categoryStore;
            _inventoryStore = inventoryStore;

            TabItems = new ObservableCollection<TabItems>();
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                LoadTabItems();
            }));
        }

        private void LoadTabItems()
        {
            TabItems.Clear();

            foreach (var category in _categorystore.Categories)
            {
                var tab = new TabItems(
                    category.Name,
                    _inventoryStore.Products,
                    product => string.IsNullOrWhiteSpace(SearchBar) || product.Name?.IndexOf(SearchBar, StringComparison.InvariantCultureIgnoreCase) >= 0
                );

                TabItems.Add(tab);
            }
        }

    }

    public class TabItems : INotifyPropertyChanged
    {
        public string Name => _category;

        private ICollectionView _inventory;
        public ICollectionView Inventory
        {
            get => _inventory;
            private set
            {
                if (_inventory != value)
                {
                    _inventory = value;
                    OnPropertyChanged(nameof(Inventory));
                }
            }
        }

        private readonly ObservableCollection<ProductDataViewModel> _allProducts;
        private readonly string _category;
        private readonly Func<ProductDataViewModel, bool> _searchFilter;

        public TabItems(string category, ObservableCollection<ProductDataViewModel> allProducts, Func<ProductDataViewModel, bool> searchFilter)
        {
            _category = category;
            _allProducts = allProducts;
            _searchFilter = searchFilter;

            // Kick off deferred loading (non-blocking)
            DeferInventoryInitialization();
        }

        private async void DeferInventoryInitialization()
        {
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Inventory = new ListCollectionView(_allProducts)
                {
                    Filter = FilterByCategory
                };
            }, System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        private bool FilterByCategory(object item)
        {
            return item is ProductDataViewModel product &&
                   product.Item.Category.Name == _category &&
                   _searchFilter(product);
        }

        public void Refresh() => _inventory?.Refresh();

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }



}
