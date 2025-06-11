using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mhyrenz_Interface.State
{
    public class InventoryStore : IInventroyStore
    {
        private readonly UndoRedoManager _undoRedoManager;
        private readonly IViewModelFactory<ProductViewModel> _productsViewModelFactory;
        private readonly IProductService _productService;
        private readonly ITransactionsService _transactionService;

        public ObservableCollection<ProductViewModel> Products { get; } = new ObservableCollection<ProductViewModel>();
        public ICommand UpdateProductCommand { get; set; }
        public ICommand PurchaseProductCommand { get; set; }
        public InventoryStore(
            UndoRedoManager undoRedoManager,
            IEnumerable<Product> products,
            IViewModelFactory<ProductViewModel> productsViewModelFactory,
            IProductService productService,
            ITransactionsService transactionsService)
        {
            _undoRedoManager = undoRedoManager;
            _productsViewModelFactory = productsViewModelFactory;
            _productService = productService;
            _transactionService = transactionsService;

            UpdateProductCommand = new UpdateProductCommand(_productService, this);
            PurchaseProductCommand = new PurchaseProductCommand(_transactionService, this);
        }

        public void LoadProducts(IEnumerable<Product> products)
        {
            Products.Clear();

            foreach (var product in products)
            {
                var viewModel = _productsViewModelFactory.CreateViewModel(product);

                var tracker = new PropertyChangeTracker<ProductViewModel>(viewModel);
                Action<string, object, object> commonCommand = (propertyName, oldValue, newValue) => {
                    _undoRedoManager.Execute(new ProductViewModelCommand(
                        viewModel,
                        propertyName,
                        oldValue,
                        newValue,
                        UpdateProductCommand
                    ));
                };
                Action<string, object, object> purchaseCommand = (propertyName, oldValue, newValue) => {
                    _undoRedoManager.Execute(new ProductViewModelCommand(
                        viewModel,
                        propertyName,
                        oldValue,
                        newValue,
                        PurchaseProductCommand
                    ));
                };

                // Track initial values
                tracker.Track(nameof(ProductViewModel.Purchase), viewModel.Purchase, purchaseCommand);
                tracker.Track(nameof(ProductViewModel.RetailPrice), viewModel.RetailPrice, commonCommand);
                tracker.Track(nameof(ProductViewModel.ListPrice), viewModel.ListPrice, commonCommand);
                tracker.Track(nameof(ProductViewModel.Qty), viewModel.Qty, commonCommand);
                tracker.Track(nameof(ProductViewModel.Name), viewModel.Name, commonCommand);
                tracker.Track(nameof(ProductViewModel.Barcode), viewModel.Barcode, commonCommand);
                tracker.Track(nameof(ProductViewModel.Expiry), viewModel.Expiry, commonCommand);
                tracker.Track(nameof(ProductViewModel.Batch), viewModel.Batch, commonCommand);
                tracker.Track(nameof(ProductViewModel.CategoryId), viewModel.CategoryId, commonCommand);
                //tracker.Track(nameof(ProductViewModel.Item), viewModel.Item);
                

                Products.Add(viewModel);
            }
        }
    }
}



//public class AppStateService
//{
//    private readonly UndoRedoManager _undoRedoManager;
//    public ObservableCollection<ProductViewModel> Products { get; } = new();

//    public AppStateService(UndoRedoManager undoRedoManager)
//    {
//        _undoRedoManager = undoRedoManager;
//    }

//    public void LoadProducts(IEnumerable<Product> products)
//    {
//        Products.Clear();

//        foreach (var product in products)
//        {
//            var viewModel = new ProductViewModel(product);

//            var tracker = new PropertyChangeTracker<ProductViewModel>(
//                viewModel,
//                (propertyName, oldValue, newValue) =>
//                {
//                    _undoRedoManager.Execute(new PropertyChangeCommand<ProductViewModel>(
//                        viewModel,
//                        propertyName,
//                        oldValue,
//                        newValue));
//                });

//            // Track initial values
//            tracker.Track(nameof(ProductViewModel.RetailPrice), viewModel.RetailPrice);
//            tracker.Track(nameof(ProductViewModel.ListPrice), viewModel.ListPrice);
//            // Track more properties as needed

//            Products.Add(viewModel);
//        }
//    }
//}




//public class AppStateService
//{
//    private readonly UndoRedoManager _undoRedoManager;

//    public ObservableCollection<ProductViewModel> Products { get; } = new();

//    public AppStateService(UndoRedoManager undoRedoManager)
//    {
//        _undoRedoManager = undoRedoManager;
//    }

//    public void LoadProducts(IEnumerable<Product> products)
//    {
//        Products.Clear();

//        foreach (var product in products)
//        {
//            var viewModel = new ProductViewModel(product);
//            viewModel.PropertyChanged += Product_PropertyChanged;
//            Products.Add(viewModel);
//        }
//    }

//    private void Product_PropertyChanged(object sender, PropertyChangedEventArgs e)
//    {
//        if (sender is not ProductViewModel productVm) return;

//        switch (e.PropertyName)
//        {
//            case nameof(ProductViewModel.RetailPrice):
//                _undoRedoManager.Execute(new ChangeProductRetailPriceCommand(
//                    productVm,
//                    oldValue: ???,  // You need a way to track old value
//                    newValue: productVm.RetailPrice));
//                break;

//                // Add other property tracking here...
//        }
//    }
//}

