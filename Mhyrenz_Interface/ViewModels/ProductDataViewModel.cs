﻿using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels.Factory;
using System;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mhyrenz_Interface.ViewModels
{
    public class ProductDataViewModelDTO
    {
        public Product Product { get; set; }
        public ISessionStore SessionStore { get; set; }
        public Action OnSessionNull { get; set; }
    }

    public class ProductDataViewModel: BaseViewModel
    {
        private readonly ISessionStore _sessionStore;
        private readonly ICategoryStore _categoryStore;
        private readonly Action _requireSession;
        public Product Item { get; set; }

        public ProductDataViewModel(ISessionStore sessionStore, ICategoryStore categoryStore, Product product)
        {
            Item = product;
            _sessionStore = sessionStore;
            _categoryStore = categoryStore;
        }

        public int NetQty { 
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

        private Brush _categoryColor;
        public Brush CategoryColor
        {
            get => _categoryColor ?? _categoryStore.Colors[CategoryId];
            set
            {
                _categoryColor = value;
                OnPropertyChanged(nameof(CategoryColor));
            }
        }
        public int CategoryId => Item.CategoryId;
        public string CategoryName => Item.Category.Name;

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