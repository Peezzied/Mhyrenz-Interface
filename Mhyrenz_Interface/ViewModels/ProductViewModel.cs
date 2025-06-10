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

        private string _name;
        public string Name 
        {
            get => Item.Name;

            set
            {
                if (SetProperty(ref _name, value, nameof(value)))
                    Item.Name = value;
            }
        }

        private int _qty;
        public int Qty
        {
            get => Item.Qty;

            set
            {
                if (SetProperty(ref _qty, value, nameof(value)))
                    Item.Qty = value;
            }
        }

        private decimal _retailPrice;
        public decimal RetailPrice
        {
            get => Item.RetailPrice;

            set
            {
                if (SetProperty(ref _retailPrice, value, nameof(value)))
                    Item.RetailPrice = value;
            }
        }

        private decimal _listPrice;
        public decimal ListPrice
        {
            get => Item.ListPrice;

            set
            {
                if (SetProperty(ref _listPrice, value, nameof(value)))
                    Item.ListPrice = value;
            }
        }

        private int? _barcode;
        public int? Barcode
        {
            get => Item.Barcode;

            set
            {
                if (SetProperty(ref _barcode, value, nameof(value)))
                    Item.Barcode = value;
            }
        }

        private DateTime? _expiry;
        public DateTime? Expiry
        {
            get => Item.Expiry;

            set
            {
                if (SetProperty(ref _expiry, value, nameof(value)))
                    Item.Expiry = value;
            }
        }

        private string _batch;
        public string Batch
        {
            get => Item.Batch;

            set
            {
                if (SetProperty(ref _batch, value, nameof(value)))
                    Item.Batch = value;
            }
        }

        private int _categoryId;
        public int CategoryId
        {
            get => Item.CategoryId;

            set
            {
                if (SetProperty(ref _categoryId, value, nameof(value)))
                    Item.CategoryId = value;
            }
        }
    }
}