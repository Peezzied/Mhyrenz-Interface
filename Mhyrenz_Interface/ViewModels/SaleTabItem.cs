using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GongSolutions.Wpf.DragDrop;
using HandyControl.Tools.Extension;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.State;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Mhyrenz_Interface.ViewModels
{
    public class SaleTabItem : BaseViewModel
    {
        public SaleTabItem(string header, Sale sale, InventoryDataGridViewModel viewModel,
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

        private void UndoRedoManager_UndoRedoEvent(ActionType arg1, UndoRedoEventArgs arg2)
        {
            if (arg2.Command is UndoRedoBoundCommand command 
                && command.Command is SaleBoundPurchaseCommand saleBoundPurchaseCommand)
            {
                AddToSale(saleBoundPurchaseCommand.CheckoutResult);
            }
        }

        public void LoadTransactions()
        {
            App.Current.BeginInvoke(new Action(async () => //TODO re - evaluate the async keyword in here
            {
                Sales.AddRange(Sale.Transactions.Select(t =>
                   new KeyValuePair<int, TransactionDataViewModel>(
                       t.ProductId,
                       _transactionDataViewModel(t))));
            }));
        }

        public override void Dispose()
        {
            _undoRedoManager.UndoRedoEvent -= UndoRedoManager_UndoRedoEvent;
        }

        private readonly CreateViewModel<TransactionDataViewModel> _transactionDataViewModel;

        public Sale Sale { get; private set; }

        private readonly IUndoRedoManager _undoRedoManager;

        public string Header { get; set; }

        // product id to transaction vm
        public ObservableDictionary<int, TransactionDataViewModel> Sales { get; } = new ObservableDictionary<int, TransactionDataViewModel>();

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

        public InventoryDataGridViewModel ContentViewModel { get; }

        public SaleDropHandler SaleDropHandler { get; }
        public InventoryDragHandler InventoryDragHandler { get; }

        public bool HasTransaction(int productId) => Sales.TryGetValue(productId, out var transaction);

        public void AddToSale(CheckoutResult checkoutResult)
        {
            if (checkoutResult.WasRemoved)
            {
                Sales.Remove(checkoutResult.Transaction.ProductId);
                return;
            }

            var transaction = checkoutResult.Transaction;

            if (Sales.TryGetValue(transaction.ProductId, out var existingVm))
            {
                existingVm.Transaction = transaction;
            }
            else
            {
                Sales.Add(transaction.ProductId, _transactionDataViewModel(transaction));
            }
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
                Method = SaleBoundPurchaseCommand.DTO.Type.Add,
                ProductId = product.Id,
                SaleId = saleTabItem.Sale.Id
            });
            saleTabItem.AddToSale(saleBoundPurchaseCommand.CheckoutResult);
        }
    }
}