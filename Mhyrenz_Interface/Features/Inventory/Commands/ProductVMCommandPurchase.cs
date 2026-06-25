using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Shared.Behaviors;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Inventory.Commands
{
    [Obsolete("Temporarily not used disabled")]
    public class ProductVMCommandPurchase : ProductVMPropertyChangeCommand
    {
        private readonly DTO _dto;
        private readonly IInventoryStore _inventoryStore;
        private readonly ICheckoutService _checkoutService;
        private readonly ITransactionStore _transactionStore;
        private int _amount;
        private CheckoutResult _result;

        public ProductVMCommandPurchase(DTO dto, IInventoryStore inventoryStore, ICheckoutService checkoutService, ITransactionStore transactionStore) : base(dto)
        {
            _dto = dto;
            _inventoryStore = inventoryStore;
            _checkoutService = checkoutService;
            _transactionStore = transactionStore;
        }

        protected override async Task CompleterHandler(BaseViewModel vm)
        {
            await base.CompleterHandler(vm);

            if (vm is InventoryViewModel inventory && inventory.InventoryDataGrid is IFlashRequestable flasher)
            {
                var transaction = _result.Transaction;

                if (_inventoryStore.Store.TryGetValue(_dto.ProductId, out var productVm))
                {
                    productVm.Purchase = _result.Transaction.Product.Purchase;
                }

                if (transaction.IsDeleted)
                {
                    _ = _transactionStore.Store.Remove(transaction.TransactionKey);
                    return;
                }

                if (_transactionStore.Store.TryGetValue(transaction.TransactionKey, out var existingVm))
                {
                    existingVm.Transaction = transaction;
                }
                else
                {
                    _ = _transactionStore.AddTransaction(transaction);
                }

                if (productVm != null)
                {
                    await flasher.RequestFlash(productVm, DataGridFlashBehavior.OperationType.Update);
                }
            }
        }

        public override async Task Command()
        {
            await base.Command();

            var newValue = PropertyChangedArgs.NewValue as int? ?? 0;
            var oldValue = PropertyChangedArgs.OldValue as int? ?? 0;

            if (newValue == oldValue)
            {
                Cancel = true;
                return;
            }

            _amount = Math.Abs(newValue - oldValue);
            var isIncrease = newValue > oldValue;

            var shouldAdd =
                Intent == ActionType.Undo
                    ? !isIncrease
                    : isIncrease;

            if (shouldAdd)
            {
                _result = await _checkoutService.AddItem(_dto.ProductId, _amount);
            }
            else
            {
                _result = await _checkoutService.Subtract(_dto.ProductId, _amount);
            }
        }

        public new class DTO : PropertyChangeCommand<ProductVMRowInfo>.DTO
        {
            public int ProductId { get; set; }
        }
    }
}
