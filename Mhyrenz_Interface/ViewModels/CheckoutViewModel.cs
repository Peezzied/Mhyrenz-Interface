using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Dragablz;
using GongSolutions.Wpf.DragDrop;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
using Microsoft.EntityFrameworkCore.Internal;

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

            App.Current.Dispatcher.BeginInvoke(new Action(async () => // TODO re-evaluate the async keyword in here
            {
                LoadTabItems();
            }));
        }

        private async void LoadTabItems()
        {
            var sales = await _checkoutService.GetActive();

            if (!sales.Any())
            {
                var sale = await _checkoutService.Create(_sessionStore.CurrentSession.Id);
                SaleTabItems.Add(_saleTabItemFactory(sale.Created_at.ToString(), _inventoryDataGridFactory(this), sale));
                return; 
            }

            SaleTabItems.AddRange(sales.Select(s =>
            {
                var saleTabItem = _saleTabItemFactory(s.Created_at.ToString(), _inventoryDataGridFactory(this), s);
                saleTabItem.LoadTransactions();
                return saleTabItem;
            }));
        }

        private readonly CreateViewModel<InventoryDataGridViewModel> _inventoryDataGridFactory;
        private readonly ISessionStore _sessionStore;
        private readonly ICheckoutService _checkoutService;
        private readonly CreateViewModel<SaleTabItem> _saleTabItemFactory;

        private SaleTabItem _selectedItem;
        public SaleTabItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem?.Dispose();

                _selectedItem = value;


                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public ObservableCollection<SaleTabItem> SaleTabItems { get; } = new ObservableCollection<SaleTabItem>();

        public Func<object> NewItemFactory
        {
            get { return () => _saleTabItemFactory("Untitled", _inventoryDataGridFactory(this)); }
        }
    }
}



