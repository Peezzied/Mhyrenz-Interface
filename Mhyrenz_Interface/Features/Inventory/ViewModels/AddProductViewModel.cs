
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.Validation;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Features.Inventory.Commands;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Inventory.ViewModels
{

    #region Custom ValidationAttributes


    #endregion

    public class AddProductViewModel : ValidationViewModel
    {
        private Category _category;
        private readonly ICategoryStore _categoryStore;
        private readonly IInventoryStore _inventoryStore;

        public AsyncRelayCommand AddCommand { get; private set; }

        public AddProductViewModel(ICategoryStore categoryStore, IInventoryStore inventoryStore, CreateCommand<AddCommand> addCommand)
        {
            _categoryStore = categoryStore;
            _inventoryStore = inventoryStore;

            AddCommand = new AsyncRelayCommand(null);
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

        private decimal _price;

        [Required]
        public decimal Price
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

        public override void Dispose()
        {
            InvokeClearValidations();
            DrawerClose = null;
            RowIntoView = null;
        }

        public event Action DrawerClose;
        public event Action<ProductDataViewModel> RowIntoView;
        public override void InvokeClearValidations()
        {
            DrawerClose?.Invoke();
            base.InvokeClearValidations();
        }

        public void RaiseRowIntoView(ProductDataViewModel item)
        {
            RowIntoView?.Invoke(item);
        }

        protected override IRaiseCanExecuteChanged SubmitActionCommand()
        {
            return AddCommand;
        }
    }
}
