using System;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Features.Checkout.ViewModels;
using Mhyrenz_Interface.Features.Checkout.Views;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Shared.Behaviors;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Checkout.Commands
{
    public class TransactionVMRowInfo : IRowInfo
    {
        public int Sale { get; set; }
        public int[] Transactions { get; set; }
    }

    public class TransactionVMCommandPurchase : PropertyChangeCommand<TransactionVMRowInfo>, ISaleBoundCommand
    {
        private readonly DTO _dto;
        private readonly IInventoryStore _inventoryStore;
        private readonly ICheckoutService _checkoutService;
        private readonly ITransactionStore _transactionStore;
        private int _amount;
        private bool _shouldAdd;
        private CheckoutResult _result;

        public int SaleId { get; }

        public TransactionVMCommandPurchase(DTO dto, IInventoryStore inventoryStore, ICheckoutService checkoutService, ITransactionStore transactionStore) : base(dto, typeof(CheckoutView))
        {
            _dto = dto;
            _inventoryStore = inventoryStore;
            _checkoutService = checkoutService;
            _transactionStore = transactionStore;
            Completer = CompleterHandler;

            SaleId = _dto.SaleId;
        }

        private async Task CompleterHandler(BaseViewModel viewModel)
        {
            if (viewModel is CheckoutViewModel checkout
                && checkout.SelectedItem is IFlashRequestable flasher
                && viewModel is IDataGridTabHost host)
            {
                host.RowIntoView(PropertyChangedArgs.RowInfo);

                if (_result.Sale != null)
                    _transactionStore.OnSaleChange(_result.Sale);

                var transaction = _result.Transaction;

                if (_inventoryStore.Store.TryGetValue(transaction.ProductId, out var product))
                {
                    product.Purchase += !_shouldAdd ? -_amount : _amount;
                }


                if (!_shouldAdd)
                {
                    if (_transactionStore.Store.TryGetValue(transaction.TransactionKey, out var vm))
                    {
                        await flasher.RequestFlash(vm, DataGridFlashBehavior.OperationType.Remove);
                        _transactionStore.Store.Remove(transaction.TransactionKey);
                    }
                    return;
                }

                if (_transactionStore.Store.TryGetValue(transaction.TransactionKey, out var existingVm))
                {
                    existingVm.Transaction = transaction;
                    await flasher.RequestFlash(existingVm, DataGridFlashBehavior.OperationType.Update);
                }
                else
                {
                    var vm = _transactionStore.AddTransaction(transaction);

                    App.Current.BeginInvoke(new Action(() => flasher.RequestFlash(vm, DataGridFlashBehavior.OperationType.New)));
                }

                _dto.TransactionId = _result.Transaction.Id;
            }
            else
            {
                Cancel = true;
            }
        }

        public override async Task Command()
        {
            var newValue = PropertyChangedArgs.NewValue as int? ?? 0;
            var oldValue = PropertyChangedArgs.OldValue as int? ?? 0;

            if (newValue == oldValue)
            {
                Cancel = true;
                return;
            }

            await base.Command();


            _amount = Math.Abs(oldValue - newValue);

            var isIncrease = newValue > oldValue;

            _shouldAdd =
                Intent == ActionType.Undo
                    ? !isIncrease
                    : isIncrease;

            if (_shouldAdd)
            {
                _result = await _checkoutService.AddItem(_dto.SaleId, _dto.ProductId, _amount);
            }
            else
            {
                _result = await _checkoutService.Subtract(_dto.SaleId, _dto.TransactionId, _amount);
            }
        }

        public new class DTO : PropertyChangeCommand<TransactionVMRowInfo>.DTO
        {
            public int SaleId { get; set; }
            public int ProductId { get; set; }
            public int TransactionId { get; set; }
        }
    }

}
