using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels.Factory;
using System;
using System.Runtime.CompilerServices;

namespace Mhyrenz_Interface.ViewModels
{
    public class ProductDataViewModel: BaseViewModel
    {
        public Product Item { get; set; }

        public ProductDataViewModel(Product product)
        {
            Item = product;
        }

        public int NetQty { 
            get => Item.NetQty;
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


        //private int _cachedPurchase;
        private int _purchase;
        public int PurchaseDefaultEdit
        {
            get => _purchase;

            set
            {
                if (_purchase != value)
                {
                    _purchase = value;
                    //_cachedPurchase += value;
                    OnPropertyChanged(nameof(PurchaseDefaultEdit));
                }
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
                    _purchaseNormal = value - Item.Purchase;
                    //_cachedPurchase += value;
                    OnPropertyChanged(nameof(PurchaseNormalEdit));
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
        public int Qty
        {
            get => Item.Qty;

            set
            {
                if (Item.Qty != value)
                {
                    Item.Qty = value;
                    OnPropertyChanged(nameof(Qty));
                }
            }
        }
        public decimal RetailPrice
        {
            get => Item.RetailPrice;

            set
            {
                if (Item.RetailPrice != value)
                {
                    Item.RetailPrice = value;
                    OnPropertyChanged(nameof(RetailPrice));
                }
            }
        }
        public decimal ListPrice
        {
            get => Item.ListPrice;

            set
            {
                if (Item.ListPrice != value)
                {
                    Item.ListPrice = value;
                    OnPropertyChanged(nameof(ListPrice));
                }
            }
        }
        public int? Barcode
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
        public int CategoryId
        {
            get => Item.CategoryId;

            set
            {
                if (Item.CategoryId != value)
                {
                    Item.CategoryId = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            ChangeTracking.IsInventoryLoaded = false;
            base.OnPropertyChanged(propertyName);
        }
    }
}