using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Dragablz;
using GongSolutions.Wpf.DragDrop;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;

namespace Mhyrenz_Interface.ViewModels
{
    public class CheckoutViewModel : NavigationViewModel
    {
        public CheckoutViewModel(INavigationServiceEx navigationServiceEx, CreateViewModel<SaleTabItem> saleTabItemFactory, CreateViewModel<InventoryDataGridViewModel> inventoryDataGridFactory) : base(navigationServiceEx)
        {
            _inventoryDataGridFactory = inventoryDataGridFactory;
            _saleTabItemFactory = saleTabItemFactory;
            SaleTabItems = new ObservableCollection<SaleTabItem>
            {
                _saleTabItemFactory("One", _inventoryDataGridFactory(this)),
                _saleTabItemFactory("Two", _inventoryDataGridFactory(this)),
                _saleTabItemFactory("Three", _inventoryDataGridFactory(this))
            };
            
        }

        private readonly CreateViewModel<InventoryDataGridViewModel> _inventoryDataGridFactory;
        private readonly CreateViewModel<SaleTabItem> _saleTabItemFactory;

        public ObservableCollection<SaleTabItem> SaleTabItems { get; }

        public Func<object> NewItemFactory
        {
            get { return () => _saleTabItemFactory("Untitled", _inventoryDataGridFactory(this)); }
        }
    }
}



