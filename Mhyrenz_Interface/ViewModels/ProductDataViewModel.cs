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
using ObservableCollections;
using RelayCommand = Mhyrenz_Interface.Core.RelayCommand;

namespace Mhyrenz_Interface.ViewModels
{
    public interface IBarcodeBound
    {
        string Barcode { get; set; }

        event Action BarcodeReceived;
        void LoadReceiver();
    }

    public class ProductDataViewModel : TrackedViewModel, IBarcodeBound
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
            //if (Item.Extras != null)
            //{
            //    Extras = new ObservableDictionary<string, PrimativeNotifyProperty<object>>(Item.Extras.ToDictionary(k => k.Key, v => new PrimativeNotifyProperty<object>(v.Value)));
            //    Extras.ValueChanged += Extras_ValueChanged;
            //}
        }
        public void LoadReceiver()
        {
            _serialBarcodeService.OnBarcodeReceived += SerialBarcodeService_OnBarcodeReceived;
        }

        private void GoToItemActionCommand(object obj)
        {
            _navigationService.Navigate(typeof(InventoryView), vm =>
            {
                var view = vm as InventoryViewModel;
                view.SelectTab(CategoryId);
                view.RowIntoView(Item.CategoryId, new[] { Item.Id });
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
            BarcodeReceived = null;
        }

        public event Action BarcodeReceived;

        public PharmaDetails PharmaDetails => Item.PharmaDetails;

        public int NetQty => Item.NetQty;

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

        private bool _isRightClicked = false;
        public bool IsRightClicked
        {
            get => _isRightClicked;
            set
            {
                _isRightClicked = value;
                OnPropertyChanged(nameof(IsRightClicked));
            }
        }

        public int PurchaseMax => Item.NetQty;

        public int PurchaseMaxNormal => Item.Qty;

        public int QtyMin => Purchase;

        //private int _cachedPurchase;
        private int _purchase;
        public int PurchaseDefaultEdit
        {
            get => _purchase;

            set
            {
                if (_purchase != value)
                {
                    SetTrackedProperty(ref _purchase, value, nameof(PurchaseDefaultEdit));

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
                    SetTrackedProperty(ref _purchaseNormal, value - Item.Purchase, nameof(PurchaseNormalEdit));

                    OnPropertyChanged(nameof(NetQty));
                    OnPropertyChanged(nameof(PurchaseMaxNormal));
                }
            }
        }

        public int Purchase => Item.Purchase;


        public string Name
        {
            get => Item.Name;

            set
            {
                if (Item.Name != value)
                {
                    SetTrackedProperty(Item.Name, value, 
                        v => Item.Name = v, nameof(Name));
                }
            }
        }

        private Supplier _supplier;
        public Supplier Supplier
        {
            get => _supplier;
            set
            {
                SetTrackedProperty(ref _supplier, value, nameof(Supplier));
            }
        }

        public int Qty
        {
            get => Item.Qty;

            set
            {
                if (Item.Qty != value)
                {
                    SetTrackedProperty(Item.Qty, value,
                        v => Item.Qty = v, nameof(Qty));

                    OnPropertyChanged(nameof(NetQty));
                }
            }
        }
        public decimal NetRetailPrice => Item.NetRetail;

        public decimal RetailPrice
        {
            get => Item.RetailPrice;
            set
            {
                if (Item.RetailPrice != value)
                {
                    SetTrackedProperty(Item.RetailPrice, value,
                       v => Item.RetailPrice = v, nameof(RetailPrice));

                    OnPropertyChanged(nameof(NetRetailPrice));
                }
            }
        }
        public decimal ListPrice => Item.ListPrice;
        
        public string Barcode
        {
            get => Item.Barcode;

            set
            {
                if (Item.Barcode != value)
                {
                    SetTrackedProperty(Item.Barcode, value,
                        v => Item.Barcode = v, nameof(Barcode));
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
                    SetTrackedProperty(Item.Expiry, value,
                        v => Item.Expiry = v, nameof(Expiry));
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
                    SetTrackedProperty(Item.Batch, value,
                        v => Item.Batch = v, nameof(Batch));
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

    }
}