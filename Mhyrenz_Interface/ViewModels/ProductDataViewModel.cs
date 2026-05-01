using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SerialBarcodeService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.Views;
using RelayCommand = Mhyrenz_Interface.Core.RelayCommand;

namespace Mhyrenz_Interface.ViewModels
{
    public interface IBarcodeBound
    {
        string Barcode { get; set; }

        event Action BarcodeReceived;
        void LoadReceiver();
    }

    public class ProductDataViewModel : BaseViewModel, IBarcodeBound
    {
        private readonly ISessionStore _sessionStore;
        private readonly ICategoryStore _categoryStore;
        private readonly ISerialBarcodeService _serialBarcodeService;
        private readonly INavigationServiceEx _navigationService;
        private readonly Action _requireSession;

        private Product _item;
        public Product Item
        {
            get => _item;
            set
            {
                _item = value;
                OnPropertyChanged(null);
            }
        }

        public ProductDataViewModel(ISessionStore sessionStore,
            ICategoryStore categoryStore,
            Product product,
            ISerialBarcodeService serialBarcodeService,
            INavigationServiceEx navigationServiceEx)
        {
            Item = product;
            _sessionStore = sessionStore;
            _categoryStore = categoryStore;
            _serialBarcodeService = serialBarcodeService;
            _navigationService = navigationServiceEx;

            GoToItemCommand = new RelayCommand(GoToItemActionCommand);
            if (Item.Extras != null)
            {
                Extras = new ObservableDictionary<string, PrimativeNotifyProperty<object>>(Item.Extras.ToDictionary(k => k.Key, v => new PrimativeNotifyProperty<object>(v.Value)));
                Extras.ValueChanged += Extras_ValueChanged;
            }
        }
        public void LoadReceiver()
        {
            _serialBarcodeService.OnBarcodeReceived += SerialBarcodeService_OnBarcodeReceived;
        }

        private void Extras_ValueChanged(object sender, ValueChangedEventArgs<string, PrimativeNotifyProperty<object>> e)
        {
            OnPropertyChanged(e.Key);
        }

        private void GoToItemActionCommand(object obj)
        {
            _navigationService.Navigate(typeof(InventoryView), vm =>
            {
                var view = vm as InventoryViewModel;
                view.SelectTab(CategoryId);
                view.RowIntoView(new[] { this });
            });
        }

        private void SerialBarcodeService_OnBarcodeReceived(string obj)
        {
            Barcode = obj;
            Dispose();
        }

        public override void Dispose()
        {
            _serialBarcodeService.OnBarcodeReceived -= SerialBarcodeService_OnBarcodeReceived;
            if (Extras != null) Extras.ValueChanged -= Extras_ValueChanged;
            BarcodeReceived = null;
        }

        public event Action BarcodeReceived;

        public int NetQty
        {
            get => Item.NetQty;
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        private bool _isCtrlClicked = false;
        public bool IsCtrlClicked
        {
            get => _isCtrlClicked;
            set
            {
                _isCtrlClicked = value;
                OnPropertyChanged(nameof(IsCtrlClicked));
            }
        }

        public int PurchaseMax
        {
            get => Item.NetQty;
        }

        public int PurchaseMaxNormal
        {
            get => Item.Qty;
        }

        public int QtyMin
        {
            get => Purchase;
        }

        //private int _cachedPurchase;
        private int _purchase;
        public int PurchaseDefaultEdit
        {
            get => _purchase;

            set
            {
                if (_purchase != value)
                {
                    if (!SessionRequire()) return;
                    _purchase = value;
                    //_cachedPurchase += value;
                    OnPropertyChanged(nameof(PurchaseDefaultEdit));
                    OnPropertyChanged(nameof(NetQty));
                    OnPropertyChanged(nameof(PurchaseMax));
                }
                _purchase = 0;
            }
        }

        private int _purchaseNormal;
        public int PurchaseNormalEdit
        {
            get => _purchaseNormal + Item.Purchase;

            set
            {
                if (Item.Purchase != value)
                {
                    if (!SessionRequire()) return;
                    _purchaseNormal = value - Item.Purchase;
                    //_cachedPurchase += value;
                    OnPropertyChanged(nameof(PurchaseNormalEdit));
                    OnPropertyChanged(nameof(NetQty));
                    OnPropertyChanged(nameof(PurchaseMaxNormal));
                }
            }
        }

        public int Purchase
        {
            get => Item.Purchase;
        }


        public string Name
        {
            get => Item.Name;

            set
            {
                if (Item.Name != value)
                {
                    Item.Name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string GenericName
        {
            get => Item.GenericName;
            set
            {
                if (Item.GenericName != value)
                {
                    Item.Name = value;
                    OnPropertyChanged(nameof(GenericName));
                }
            }
        }

        private Supplier _supplier;
        public Supplier Supplier
        {
            get => _supplier;
            set
            {
                _supplier = value;
                OnPropertyChanged(nameof(Supplier));
            }
        }

        public int Qty
        {
            get => Item.Qty;

            set
            {
                if (Item.Qty != value)
                {
                    if (!SessionRequire()) return;
                    Item.Qty = value;
                    OnPropertyChanged(nameof(Qty));
                    OnPropertyChanged(nameof(NetQty));
                }
            }
        }
        public decimal NetRetailPrice
        {
            get => Item.NetRetail;
        }
        public decimal RetailPrice
        {
            get => Item.RetailPrice;
            set
            {
                if (Item.RetailPrice != value)
                {
                    if (!SessionRequire()) return;
                    Item.RetailPrice = value;
                    OnPropertyChanged(nameof(RetailPrice));
                    OnPropertyChanged(nameof(NetRetailPrice));
                }
            }
        }
        public decimal ListPrice
        {
            get => Item.ListPrice;
        }
        public string Barcode
        {
            get => Item.Barcode;

            set
            {
                if (Item.Barcode != value)
                {
                    Item.Barcode = value;
                    OnPropertyChanged(nameof(Barcode));
                }
            }
        }
        public DateTime? Expiry
        {
            get => Item.Expiry;

            set
            {
                if (Item.Expiry != value)
                {
                    Item.Expiry = value;
                    OnPropertyChanged(nameof(Expiry));
                }
            }
        }
        public string Batch
        {
            get => Item.Batch;

            set
            {
                if (Item.Batch != value)
                {
                    Item.Batch = value;
                    OnPropertyChanged(nameof(Batch));
                }
            }
        }

        private Brush GetColor()
        {
            if (_categoryStore.Colors.TryGetValue(CategoryId, out var color)) return color;
            return null;
        }

        private Brush _categoryColor;
        public Brush CategoryColor
        {
            get => _categoryColor ?? GetColor() ?? Brushes.Red;
            set
            {
                _categoryColor = value;
                OnPropertyChanged(nameof(CategoryColor));
            }
        }
        public int CategoryId => Item.CategoryId;
        public string CategoryName => Item.Category.Name;

        public ObservableDictionary<string, PrimativeNotifyProperty<object>> Extras { get; private set; }

        public ICommand GoToItemCommand { get; }

        private bool SessionRequire()
        {
            if (_sessionStore.CurrentSession is Session)
                return true;
            else
            {
                _requireSession?.Invoke();
                return false;
            }
        }

    }
}