using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.ViewModels.Factory;
using System;

namespace Mhyrenz_Interface.ViewModels
{
    public class ProductViewModel: NavigationViewModel
    {
        public Product Item { get; set; }

        public ProductViewModel(Product product)
        {
            Item = product;
        }
        
        private int _purchase;
        public int Purchase
        {
            get => Item.Purchase;

            set
            {
                if (Item.Purchase != value)
                {
                    _purchase = value;
                    OnPropertyChanged(nameof(Purchase));
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
    }
}