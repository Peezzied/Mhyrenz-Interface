using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Util;
using System.Windows;
using System.Windows.Controls;
using GongSolutions.Wpf.DragDrop;
using HandyControl.Tools.Extension;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Controls;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.State;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Mhyrenz_Interface.ViewModels
{
    public class SaleTabItem : BaseViewModel, IEditCancelState
    {
        private readonly CreateViewModel<TransactionDataViewModel> _transactionDataViewModel;
        private readonly IUndoRedoManager _undoRedoManager;

        private readonly Dictionary<int, TransactionDataViewModel> _salesByTransactionId =
            new Dictionary<int, TransactionDataViewModel>();

        public SaleTabItem(
            string header,
            Sale sale,
            InventoryDataGridViewModel viewModel,
            IUndoRedoManager undoRedoManager,
            CreateViewModel<TransactionDataViewModel> transactionDataViewModel,
            CreateCommand<SaleBoundPurchaseCommand> saleBoundPurchaseCommand)
        {
            _transactionDataViewModel = transactionDataViewModel;
            _undoRedoManager = undoRedoManager;

            Sale = sale;
            Header = header;
            ContentViewModel = viewModel;

            SaleDropHandler = new SaleDropHandler(this, saleBoundPurchaseCommand);
            InventoryDragHandler = new InventoryDragHandler(this);

            _undoRedoManager.UndoRedoEvent += UndoRedoManager_UndoRedoEvent;
        }

        public Sale Sale { get; private set; }

        public string Header { get; set; }

        public ObservableCollection<TransactionDataViewModel> Sales { get; } =
            new ObservableCollection<TransactionDataViewModel>();

        public InventoryDataGridViewModel ContentViewModel { get; }

        public SaleDropHandler SaleDropHandler { get; }

        public InventoryDragHandler InventoryDragHandler { get; }

        private DataGridRowDetailsVisibilityMode _productRowDetailsVisibilityMode =
            DataGridRowDetailsVisibilityMode.VisibleWhenSelected;

        public DataGridRowDetailsVisibilityMode ProductRowDetailsVisibilityMode
        {
            get => _productRowDetailsVisibilityMode;
            set
            {
                _productRowDetailsVisibilityMode = value;
                OnPropertyChanged(nameof(ProductRowDetailsVisibilityMode));
            }
        }

        public bool IsEditCancelled { get; set; }

        public void LoadTransactions()
        {
            Sales.Clear();
            _salesByTransactionId.Clear();

            foreach (var transaction in Sale.Transactions)
            {
                var vm = _transactionDataViewModel(transaction);

                _salesByTransactionId[transaction.Id] = vm;
                Sales.Add(vm);
            }
        }

        public bool HasTransaction(int transactionId)
        {
            return _salesByTransactionId.ContainsKey(transactionId);
        }

        public async void AddToSale((CheckoutResult result, SaleBoundPurchaseCommand.DTO.Type method) checkout)
        {
            var checkoutResult = checkout.result;

            if (checkoutResult.Sale?.Id != Sale.Id)
                return;

            if (checkoutResult.WasRemoved)
            {
                var transactionId = checkoutResult.Transaction.Id;
                if (_salesByTransactionId.TryGetValue(transactionId, out var vm))
                {
                    await vm.RequestFlash(checkout.method);
                    _salesByTransactionId.Remove(transactionId);
                    Sales.Remove(vm);
                }

                return;
            }

            var transaction = checkoutResult.Transaction;

            if (transaction == null)
                return;

            if (_salesByTransactionId.TryGetValue(transaction.Id, out var existingVm))
            {
                checkout.method = SaleBoundPurchaseCommand.DTO.Type.Add;
                existingVm.Transaction = transaction;
                await existingVm.RequestFlash(checkout.method);
            }
            else
            {
                var vm = _transactionDataViewModel(transaction);

                _salesByTransactionId[transaction.Id] = vm;
                Sales.Add(vm);

                checkout.method = SaleBoundPurchaseCommand.DTO.Type.AddNew;
                App.Current.BeginInvoke(new Action(() => vm.RequestFlash(checkout.method)));
            }
        }

        private void UndoRedoManager_UndoRedoEvent(ActionType actionType, UndoRedoEventArgs args)
        {
            if (args.Command is UndoRedoBoundCommand command &&
                command.Command is SaleBoundPurchaseCommand saleCommand)
            {
                AddToSale(saleCommand.Result);
            }
        }

        public override void Dispose()
        {
            _undoRedoManager.UndoRedoEvent -= UndoRedoManager_UndoRedoEvent;
            base.Dispose();
        }
    }

    public class InventoryDragHandler : DefaultDragHandler
    {
        public InventoryDragHandler(SaleTabItem saleTabItem)
        {
            SaleTabItem = saleTabItem;
        }

        public SaleTabItem SaleTabItem { get; }

        public override void StartDrag(IDragInfo dragInfo)
        {
            if (dragInfo.SourceItem is ProductDataViewModel product)
            {
                SaleTabItem.ProductRowDetailsVisibilityMode =
                    DataGridRowDetailsVisibilityMode.Collapsed;

                dragInfo.Data = product;
                dragInfo.Effects = DragDropEffects.Copy;
            }
        }

        public override void DragDropOperationFinished(
            DragDropEffects operationResult,
            IDragInfo dragInfo)
        {
            SaleTabItem.ProductRowDetailsVisibilityMode =
                DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
        }
    }

    public class SaleDropHandler : DefaultDropHandler
    {
        private readonly SaleTabItem saleTabItem;
        private readonly CreateCommand<SaleBoundPurchaseCommand> _saleBoundPurchaseCommand;

        public SaleDropHandler(SaleTabItem saleTabItem, CreateCommand<SaleBoundPurchaseCommand> saleBoundPurchaseCommand)
        {
            this.saleTabItem = saleTabItem;
            _saleBoundPurchaseCommand = saleBoundPurchaseCommand;
        }

        public override void DragOver(IDropInfo dropInfo)
        {
            base.DragOver(dropInfo);
            if (dropInfo.Data is ProductDataViewModel)
            {
                dropInfo.Effects = DragDropEffects.Copy;
                dropInfo.DropTargetAdorner = null;
            }
        }

        public override void Drop(IDropInfo dropInfo)
        {
            var product = (dropInfo.Data as ProductDataViewModel).Item;
            var saleBoundPurchaseCommand = _saleBoundPurchaseCommand();
            saleBoundPurchaseCommand.Execute(new SaleBoundPurchaseCommand.DTO
            {
                Amount = 1,
                Method = SaleBoundPurchaseCommand.DTO.Type.AddNew,
                ProductId = product.Id,
                SaleId = saleTabItem.Sale.Id
            });
            saleTabItem.AddToSale(saleBoundPurchaseCommand.Result);
        }
    }
}