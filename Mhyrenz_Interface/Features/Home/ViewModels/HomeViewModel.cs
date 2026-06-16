using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Features.Checkout.ViewModels;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Home.ViewModels
{
    public class HomeViewModel : NavigationViewModel, IAsyncInitializable
    {
        private readonly ICheckoutService _checkoutService;
        private readonly ITransactionStore _transactionStore;

        public CompletedSaleViewModel CompletedSaleViewModel { get; }
        public ActionViewModel ActionViewModel { get; }

        private InventoryDataGridViewModel _invetoryDataGridContext;
        public InventoryDataGridViewModel InventoryDataGridContext
        {
            get => _invetoryDataGridContext;
            set => SetProperty(ref _invetoryDataGridContext, value);
        }

        public OverviewChartViewModel OverviewChartViewModel { get; }
        public string Bindtest { get; private set; }

        private decimal _sales;
        public decimal Sales
        {
            get => _sales;
            set => SetProperty(ref _sales, value);
        }

        private decimal _profit;
        public decimal Profit
        {
            get => _profit;
            set => SetProperty(ref _profit, value);
        }

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


        public HomeViewModel(ICheckoutService checkoutService,
            ITransactionStore transactionStore,
            IInventoryStore inventroyStore,
            ICategoryStore categoryStore,
            INavigationServiceEx navigationServiceEx,
            OverviewChartViewModel overviewChartViewModel,
            ActionViewModel actionViewModel,
            CompletedSaleViewModel completedSaleViewModel) : base(navigationServiceEx)
        {
            _checkoutService = checkoutService;
            _transactionStore = transactionStore;

            OverviewChartViewModel = overviewChartViewModel;
            CompletedSaleViewModel = completedSaleViewModel;
            ActionViewModel = actionViewModel;

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (categoryStore.Colors.Count != 0)
                    foreach (var item in inventroyStore.Store)
                    {
                        item.CategoryColor = categoryStore.Colors[item.CategoryId];
                    }

                var topCategory = OverviewChartViewModel.CategoryChartData
                    .Where(c => c.Sales.Value > 0)
                    .OrderByDescending(c => c.Sales.Value)
                    .FirstOrDefault()?.Category;

                if (topCategory == null)
                    return;

                CategoryName = topCategory.Name;
                CategoryColor = categoryStore.Colors[topCategory.Id];
            }), DispatcherPriority.ContextIdle);
        }

        public async Task InitializeAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            Profit = _transactionStore.Store.Sum(t =>
                Transaction.CalculateProfit(
                    t.RetailPrice,
                    t.Transaction.CostPrice,
                    t.Qty));

            token.ThrowIfCancellationRequested();

            var history = await _checkoutService.GetHistory();

            token.ThrowIfCancellationRequested();

            Sales = history.Sum(s => s.Total);
        }

        public override void Dispose()
        {
            CompletedSaleViewModel.Dispose();
            OverviewChartViewModel.Dispose();
        }
    }
}
