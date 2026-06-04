using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dragablz;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels.Factory;
using Microsoft.EntityFrameworkCore.Internal;
using ObservableCollections;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Mhyrenz_Interface.ViewModels
{
    public class CheckoutViewModel : NavigationViewModel
    {

        public CheckoutViewModel(INavigationServiceEx navigationServiceEx, ICheckoutService checkoutService,
            ISessionStore sessionStore,
            IInventoryStore inventoryStore,
            ShellViewModel shellViewModel,
            CreateViewModel<CompletedSaleViewModel> completedSaleViewModel,
            CreateViewModel<SaleTabItem> saleTabItemFactory,
            CreateViewModel<InventoryDataGridViewModel> inventoryDataGridFactory) : base(navigationServiceEx)
        {
            _shellViewModel = shellViewModel;
            shellViewModel.RibbonBarViewModel = this;

            _inventoryDataGridFactory = inventoryDataGridFactory;
            _sessionStore = sessionStore;
            _inventoryStore = inventoryStore;
            _checkoutService = checkoutService;
            _saleTabItemFactory = saleTabItemFactory;

            _completedSaleViewModel = completedSaleViewModel;

            AddSaleCommand = new AsyncRelayCommand(CreateSale);

            App.Current.Dispatcher.BeginInvoke(new Action(async () => // TODO re-evaluate the async keyword in here
            {
                LoadTabItems();
            }));
        }

        private async Task CreateSale(object arg)
        {
            await CreateSale();
        }

        private async void LoadTabItems()
        {
            var sales = await _checkoutService.GetActive();
            _startSaleCount = await _checkoutService.InactiveTransactionsCount();

            if (!sales.Any())
            {
                await CreateSale();
                return;
            }

            var inventoryDataGrid = _inventoryDataGridFactory(this);
            inventoryDataGrid.InventoryView.AttachFilter(e => e.NetQty > 0);
            inventoryDataGrid.IsReadOnly = true;

            _inventoryStore.PurchaseEvent += InventoryStore_PurchaseEvent;

            SaleTabItems.AddRange(sales.Select(s =>
            {
                var saleTabItem = _saleTabItemFactory(
                    this,
                    s.FromStartCount(_startSaleCount) + " Regular Customer",
                    inventoryDataGrid,
                    s);

                return saleTabItem;
            }));
        }

        public async void DropCurrentTab(SaleTabItem saleTabItem, bool asCompleted)
        {
            await _checkoutService.DiscardSale(saleTabItem.Sale.Id, asCompleted);
            await CreateOrIgnore();

            saleTabItem.Dispose();
            SaleTabItems.Remove(saleTabItem);
        }

        private void InventoryStore_PurchaseEvent(object sender, InventoryStoreEventArgs e)
        {
            SelectedItem.InventoryDataGridViewModel.InventoryView
                .AttachFilter(p => p.NetQty > 0);
        }

        private readonly HashSet<int> _initializedTabs = new HashSet<int>();
        private readonly CreateViewModel<InventoryDataGridViewModel> _inventoryDataGridFactory;
        private readonly ISessionStore _sessionStore;
        private readonly IInventoryStore _inventoryStore;
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
                _selectedItem?.Dispose();

                _selectedItem = value;

                if (_selectedItem != null
                    && !_initializedTabs.Contains(_selectedItem.Sale.Id))
                {
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _selectedItem.LoadTransactions();
                    }), DispatcherPriority.Background);
                    _initializedTabs.Add(_selectedItem.Sale.Id);
                }

                OnPropertyChanged(nameof(SelectedItem));
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

        private CompletedSaleViewModel completedSaleViewModel;
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

        private async void ClosingTab(ItemActionCallbackArgs<TabablzControl> args)
        {
            if (args.DragablzItem.DataContext is SaleTabItem saleTabItem && ClosingPrompt(saleTabItem))
                DropCurrentTab(saleTabItem, asCompleted: false);
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

            if (saleTabItem.Transactions.Count > 0)
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
            SaleTabItem item = _saleTabItemFactory(this,
                sale.FromStartCount(_startSaleCount) + " Regular Customer",
                _inventoryDataGridFactory(this),
                sale);

            SaleTabItems.Add(item);
            SelectedItem = item;
        }

        public override void Dispose()
        {
            _inventoryStore.PurchaseEvent -= InventoryStore_PurchaseEvent;
            _shellViewModel.RibbonBarViewModel = null;  
        }
    }
}



