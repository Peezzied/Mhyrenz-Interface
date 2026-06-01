using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Dragablz;
using GongSolutions.Wpf.DragDrop;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
using Microsoft.EntityFrameworkCore.Internal;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Mhyrenz_Interface.ViewModels
{
    public class CheckoutViewModel : NavigationViewModel
    {

        public CheckoutViewModel(INavigationServiceEx navigationServiceEx, ICheckoutService checkoutService,
            ISessionStore sessionStore,
            CreateViewModel<SaleTabItem> saleTabItemFactory,
            CreateViewModel<InventoryDataGridViewModel> inventoryDataGridFactory) : base(navigationServiceEx)
        {
            _inventoryDataGridFactory = inventoryDataGridFactory;
            _sessionStore = sessionStore;
            _checkoutService = checkoutService;
            _saleTabItemFactory = saleTabItemFactory;

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

            SaleTabItems.AddRange(sales.Select(s =>
            {
                var saleTabItem = _saleTabItemFactory(
                    s.FromStartCount(_startSaleCount) + " Regular Customer",
                    inventoryDataGrid,
                    s);

                return saleTabItem;
            }));
        }

        private readonly HashSet<int> _initializedTabs = new HashSet<int>();
        private readonly CreateViewModel<InventoryDataGridViewModel> _inventoryDataGridFactory;
        private readonly ISessionStore _sessionStore;
        private readonly ICheckoutService _checkoutService;
        private readonly CreateViewModel<SaleTabItem> _saleTabItemFactory;

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
                    && !_initializedTabs.Contains(_selectedItem.Sale.Id)
                    && _selectedItem.Sale.Transactions.Count > 0)
                {
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _selectedItem.LoadTransactions(); // FIXME delayed Transactions binding
                    }), DispatcherPriority.Background);
                    _initializedTabs.Add(_selectedItem.Sale.Id);
                }

                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public ObservableCollection<SaleTabItem> SaleTabItems { get; } = new ObservableCollection<SaleTabItem>();

        public ItemActionCallback OnTabClosing => ClosingTab;

        private async void ClosingTab(ItemActionCallbackArgs<TabablzControl> args)
        {
            if (args.DragablzItem.DataContext is SaleTabItem saleTabItem)
            {
                MessageBoxResult firstPrompt = MessageBox.Show(
                    "You have unsaved changes. Closing this tab will discard the current sale.",
                    "Unsaved Changes",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (firstPrompt != MessageBoxResult.Yes)
                {
                    args.Cancel();
                    return;
                }

                if (saleTabItem.Transactions.Count > 0)
                {
                    MessageBoxResult secondPrompt = MessageBox.Show(
                        "This action cannot be undone.  \nAre you sure you want to permanently discard this sale?",
                        "Confirm Discard Sale",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (secondPrompt != MessageBoxResult.Yes)
                    {
                        args.Cancel();
                        return;
                    }
                }

                await _checkoutService.DiscardSale(saleTabItem.Sale.Id);
                saleTabItem.Dispose();

                if (args.Owner.Items.Count == 1)
                {
                    await CreateSale();
                }
            }
        }

        private async Task CreateSale()
        {
            var sale = await _checkoutService.Create(_sessionStore.CurrentSession.Id);
            var item = _saleTabItemFactory(sale.FromStartCount(_startSaleCount) + " Regular Customer", _inventoryDataGridFactory(this), sale);

            SaleTabItems.Add(item);
            SelectedItem = item;
        }
    }
}



