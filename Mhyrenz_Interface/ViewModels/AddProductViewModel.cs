
using HandyControl.Tools.Extension;
using LiveChartsCore.Kernel;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Core.ValidationAttributes;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.State;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mhyrenz_Interface.ViewModels
{

    #region Custom ValidationAttributes


    #endregion

    public class AddProductViewModel : ValidationViewModel
    {
        private Category _category;
        private readonly ICategoryStore _categoryStore;

        public AddProductViewModel(ICategoryStore categoryStore, IProductService productService, IInventoryStore inventoryStore)
        {
            ClearValidations = InvokeClearValidations;
            _categoryStore = categoryStore;

            Categories.AddRange(_categoryStore.Categories.Select(c => c.Key));

            base.SubmitActionCommand = new AddCommand(this, productService, inventoryStore);
        }

        public ObservableCollection<Category> Categories { get; private set; } = new ObservableCollection<Category>();

        #region "Properties"
        private bool _validationHasError;

        [MustBeFalse]
        public bool ValidationHasError
        {
            get => _validationHasError;
            set
            {
                _validationHasError = value;
                Validate(nameof(ValidationHasError), value);
                OnPropertyChanged(nameof(ValidationHasError));
            }
        }

        private bool _isGeneric;    
        public bool IsGeneric
        {
            get => _isGeneric;
            set
            {
                _isGeneric = value;
                OnPropertyChanged(nameof(IsGeneric));
            }
        }

        [Required]
        public Category SelectedCategory
        {
            get => _category;
            set
            {
                _category = value;

                IsGeneric = false;
                if (_category.Name == "Generic") // HARDCODING CRITICAL SECTION
                    IsGeneric = true;

                Validate(nameof(SelectedCategory), value);
                OnPropertyChanged(nameof(SelectedCategory));
            }
        }

        private string _name;

        [Required]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                Validate(nameof(Name), value);
                OnPropertyChanged(nameof(Name));
            }
        }

        private string _genericName;
        public string GenericName
        {
            get => _genericName;
            set
            {
                _genericName = value;
                OnPropertyChanged(nameof(GenericName));
            }
        }

        private int _qty;

        [Required]
        public int Qty
        {
            get => _qty;
            set
            {
                _qty = value;
                Validate(nameof(Qty), value);
                OnPropertyChanged(nameof(Qty));
            }
        }

        private double _price;

        [Required]
        public double Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged(nameof(Price));
            }
        }

        private DateTime _expiry = DateTime.Now;
        public DateTime Expiry
        {
            get => _expiry;
            set
            {
                _expiry = value;
                Validate(nameof(Expiry), value);
                OnPropertyChanged(nameof(Expiry));
            }
        }

        private string _batch;
        public string Batch
        {
            get => _batch;
            set
            {
                _batch = value;
                OnPropertyChanged(nameof(Batch));
            }
        }

        private string _supplier;
        public string Supplier
        {
            get => _supplier;
            set
            {
                _supplier = value;
                OnPropertyChanged(nameof(Supplier));
            }
        }   

        private string _barcode;

        [MaxLength(13, ErrorMessage = "Invalid Barcode")]
        public string Barcode
        {
            get => _barcode;
            set
            {
                _barcode = value;
                Validate(nameof(Barcode), value);
                OnPropertyChanged(nameof(Barcode));
            }
        }

        public DateTime MinDate => DateTime.Now;
        #endregion

        public event Action DrawerClose;
        public override void InvokeClearValidations()
        {
            DrawerClose?.Invoke();
            base.InvokeClearValidations();
        }
    }
}
