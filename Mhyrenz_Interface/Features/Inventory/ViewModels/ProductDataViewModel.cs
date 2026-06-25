using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SerialBarcodeService;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Shared.Behaviors;
using Mhyrenz_Interface.Store;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Mhyrenz_Interface.Features.Inventory.ViewModels
{
    public interface IBarcodeBound
    {
        string Barcode { get; set; }

        event Action BarcodeReceived;
        void LoadBarcodeReceiver();
        void UnloadBarcodeReceiver();
    }

    public class ProductDataViewModel : TrackedViewModel, IBarcodeBound, IFlashReceiver
    {
        private readonly ISessionStore _sessionStore;
        private readonly ICategoryStore _categoryStore;
        private readonly ISerialBarcodeService _serialBarcodeService;
        private readonly INavigationServiceEx _navigationService;
        private readonly ITransactionStore _transactionStore;

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
            ITransactionStore transactionStore,
            ISerialBarcodeService serialBarcodeService,
            INavigationServiceEx navigationServiceEx,
            PharmaDetailsViewModel pharmaDetailsViewModel)
        {
            Item = product;

            _purchaseNormal = Item.Purchase;

            _transactionStore = transactionStore;
            _sessionStore = sessionStore;
            _categoryStore = categoryStore;
            _serialBarcodeService = serialBarcodeService;
            _navigationService = navigationServiceEx;

            if (product.Category.IsPharma)
            {
                PharmaDetails = pharmaDetailsViewModel;
                PharmaDetails.TrackedPropertyChanged += PharmaDetails_TrackedPropertyChanged;
            }

            GoToItemCommand = new AsyncRelayCommand(GoToItemActionCommand);
        }

        private void PharmaDetails_TrackedPropertyChanged(object sender, TrackedPropertyChangedEventArgs e)
        {
            OnTrackedPropertyChanged(e.OldValue, e.PropertyName);
        }

        public void LoadBarcodeReceiver()
        {
            _serialBarcodeService.OnBarcodeReceived += SerialBarcodeService_OnBarcodeReceived;
        }

        public void UnloadBarcodeReceiver()
        {
            _serialBarcodeService.OnBarcodeReceived -= SerialBarcodeService_OnBarcodeReceived;
            BarcodeReceived = null;
        }

        private async Task GoToItemActionCommand(object obj)
        {
            // TODO GoToItemActionCommand comply to new post navigation signature
            //await _navigationService.NavigateAsync(typeof(InventoryView), vm =>
            //{
            //    var view = vm as InventoryViewModel;
            //    view.SelectTab(CategoryId);
            //    view.RowIntoView(Item.CategoryId, new[] { Item.Id });
            //});
        }

        private void SerialBarcodeService_OnBarcodeReceived(string code)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Barcode = code;
                BarcodeReceived?.Invoke();
                //UnloadBarcodeReceiver();
            });
        }


        public override void Dispose()
        {
            // TODO
        }

        public event Action BarcodeReceived;

        private bool _hasActiveSale = false;
        public bool HasActiveSale
        {
            get => _hasActiveSale;
            set
            {
                _hasActiveSale = value;
                OnPropertyChanged(nameof(HasActiveSale));
            }
        }

        public PharmaDetailsViewModel PharmaDetails { get; set; }

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

        public int PurchaseMax => Item.NetQty;

        public int PurchaseMaxNormal => Item.Qty;

        public int QtyMin => Purchase;

        private int _purchase;
        public int PurchaseDefaultEdit
        {
            get => _purchase;

            set
            {
                if (_purchase != value)
                {
                    SetTrackedProperty(ref _purchase, value, nameof(PurchaseDefaultEdit));
                    OnPropertyChanged(null);
                }
                _purchase = 0;
            }
        }

        private int _purchaseNormal;
        public int PurchaseNormalEdit
        {
            get => _purchaseNormal;

            set
            {
                if (Item.Purchase != value)
                {
                    SetTrackedProperty(ref _purchaseNormal, value, nameof(PurchaseNormalEdit));
                    OnPropertyChanged(null);
                }
            }
        }

        public int Purchase
        {
            get => Item.Purchase;
            set
            {
                if (Item.Purchase != value)
                {
                    Item.Purchase = value;
                    _purchaseNormal = Item.Purchase;
                    OnPropertyChanged(null);
                }
            }
        }

        public decimal MarkupRate
        {
            get => Item.MarkupRate;
            set
            {
                if (Item.MarkupRate != value && EnsurePurchaseRigidity())
                {
                    SetTrackedProperty(Item.MarkupRate, value,
                        v => Item.MarkupRate = v, nameof(MarkupRate));

                    Item.SetMarkupRate(value);
                    OnPropertyChanged(nameof(RetailPrice));
                }
            }
        }

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

        public decimal Sales
        {
            get => Item.Sales;
            set
            {
                if (Item.Sales != value)
                {
                    Item.Sales = value;
                    OnPropertyChanged(nameof(Sales));
                }
            }
        }

        public decimal RetailPrice
        {
            get => Item.RetailPrice;
            set
            {
                if (Item.RetailPrice != value && EnsurePurchaseRigidity())
                {
                    SetTrackedProperty(Item.RetailPrice, value,
                       v => Item.RetailPrice = v, nameof(RetailPrice));

                    OnPropertyChanged(nameof(Sales));
                }
            }
        }

        private bool EnsurePurchaseRigidity()
        {
            if (Purchase > 0)
            {
                var result = MessageBox.Show(
                    "This change will only affect future sales. Previous sales and transactions that have already been recorded will not be updated.\n\nDo you want to continue?",
                    "Item Price Change",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                return result == MessageBoxResult.Yes;
            }
            return true;
        }

        public decimal CostPrice
        {
            get => Item.CostPrice;
            set
            {
                if (Item.CostPrice != value && EnsurePurchaseRigidity())
                {
                    SetTrackedProperty(Item.CostPrice, value,
                       v => Item.CostPrice = v, nameof(CostPrice));

                    Item.SetMarkupRate(MarkupRate);
                    OnPropertyChanged(nameof(RetailPrice));
                }
            }
        }

        public string Barcode
        {
            get => Item.Barcode;

            set
            {
                if (Item.Barcode != value)
                {
                    DeferSetTrackedProperty(Item.Barcode, value, nameof(Barcode));
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

        public ICommand GoToItemCommand { get; }

        public override void SetValue(string propertyName, object value)
        {
            switch (propertyName)
            {
                case nameof(Barcode):
                    Item.Barcode = value as string;
                    break;
                default:
                    return;
            }

            base.SetValue(propertyName, value);
        }
    }
}