using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Features.Checkout.ViewModels;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Home.ViewModels
{
    public class HomeViewModel : NavigationViewModel, ISalesRegisterHost
    {
        private readonly IInventoryStore _inventoryStore;
        private readonly ICategoryStore _categoryStore;
        private readonly ISessionStore _sessionStore;

        public CompletedSaleViewModel CompletedSaleViewModel { get; }
        public ActionViewModel ActionViewModel { get; }

        private InventoryDataGridViewModel _invetoryDataGridContext;
        public InventoryDataGridViewModel InventoryDataGridContext
        {
            get => _invetoryDataGridContext;
            set
            {
                _invetoryDataGridContext = value;
                OnPropertyChanged(nameof(InventoryDataGridContext));
            }
        }

        public OverviewChartViewModel OverviewChartViewModel { get; }
        public string Bindtest { get; private set; }

        private bool _isRegistering;
        public bool IsRegistering
        {
            get => _isRegistering;
            set
            {
                _isRegistering = value;
                OnPropertyChanged(nameof(IsRegistering));
            }
        }

        public decimal Profit => _inventoryStore.Store.Sum(p => p.NetRetailPrice);
        public decimal Sales => _inventoryStore.Store.Sum(p => p.Item.Profit);
        public int Customers => CompletedSaleViewModel.CompletedSales.Count;

        private Brush _categoryColor;
        public Brush CategoryColor
        {
            get => _categoryColor;
            set
            {
                _categoryColor = value;
                OnPropertyChanged(nameof(CategoryColor));
            }
        }

        private string _categoryName;
        public string CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                OnPropertyChanged(nameof(CategoryName));
            }
        }


        public HomeViewModel(
            IInventoryStore inventroyStore,
            ICategoryStore categoryStore,
            ISessionStore sessionStore,
            INavigationServiceEx navigationServiceEx,
            OverviewChartViewModel overviewChartViewModel,
            ActionViewModel actionViewModel,
            CompletedSaleViewModel completedSaleViewModel) : base(navigationServiceEx)
        {
            _inventoryStore = inventroyStore;
            _categoryStore = categoryStore;
            _sessionStore = sessionStore;

            OverviewChartViewModel = overviewChartViewModel;
            CompletedSaleViewModel = completedSaleViewModel;
            ActionViewModel = actionViewModel;

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_categoryStore.Colors.Count != 0)
                    foreach (var item in _inventoryStore.Store)
                    {
                        item.CategoryColor = _categoryStore.Colors[item.CategoryId];
                    }

                var topCategory = OverviewChartViewModel.CategoryChartData
                    .Where(c => c.Sales.Value > 0)
                    .OrderByDescending(c => c.Sales.Value)
                    .FirstOrDefault()?.Category;

                if (topCategory == null)
                    return;

                CategoryName = topCategory.Name;
                CategoryColor = _categoryStore.Colors[topCategory.Id];
            }), DispatcherPriority.ContextIdle);
        }

        public override void Dispose()
        {
            CompletedSaleViewModel.Dispose();
            OverviewChartViewModel.Dispose();
        }
    }

    public interface ISalesRegisterHost
    {
        bool IsRegistering { get; set; }
    }
}
