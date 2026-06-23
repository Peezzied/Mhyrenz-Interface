using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SerialBarcodeService;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Shared.Behaviors;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Inventory.ViewModels
{
    public interface IBarcodeBound
    {
        string Barcode { get; set; }

        event Action BarcodeReceived;
        void LoadReceiver();
    }

    public class ProductDataViewModel : TrackedViewModel, IBarcodeBound, IFlashRequestable
    {
        private readonly ISessionStore _sessionStore;
        private readonly ICategoryStore _categoryStore;
        private readonly ISerialBarcodeService _serialBarcodeService;
        private readonly INavigationServiceEx _navigationService;
        private readonly ITransactionStore _transactionStore;
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
            INavigationServiceEx navigationServiceEx,
            PharmaDetailsViewModel pharmaDetailsViewModel)
        {
            Item = product;

            _purchaseNormal = Item.Purchase;

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

        public void LoadReceiver()
        {
            _serialBarcodeService.OnBarcodeReceived += SerialBarcodeService_OnBarcodeReceived;
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
        public event EventHandler<RowFlashRequestedEventArgs> FlashRequested;

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
            internal set
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
                if (Item.MarkupRate != value)
                {
                    SetTrackedProperty(Item.MarkupRate, value,
                        v => Item.MarkupRate = v, nameof(MarkupRate));
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
        public decimal CostPrice
        {
            get => Item.CostPrice;
            set
            {
                if (Item.CostPrice != value)
                {
                    SetTrackedProperty(Item.CostPrice, value,
                       v => Item.CostPrice = v, nameof(CostPrice));
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

        public Task RequestFlash(DataGridFlashBehavior.OperationType type)
        {
            var args = new RowFlashRequestedEventArgs(type);
            FlashRequested?.Invoke(this, args);

            return args.Completion.Task;
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

    }
}