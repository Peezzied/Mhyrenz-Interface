using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Mhyrenz_Interface.Commands
{
    public class TransactionVMRowInfo
    {
        public int Sale { get; set; }
        public int[] Transactions { get; set; }
    }

    public class TransactionVMCommandPurchase : PropertyChangeCommand<TransactionVMRowInfo>
    {
        private readonly DTO _dto;
        private readonly ICheckoutService _checkoutService;
        private readonly ITransactionStore _transactionStore;

        public TransactionVMCommandPurchase(DTO dto, ICheckoutService checkoutService, ITransactionStore transactionStore) : base(dto)
        {
            _dto = dto;
            _checkoutService = checkoutService;
            _transactionStore = transactionStore;
            SideEffect = SideEffectHandler;
        }

        private void SideEffectHandler(NavigationViewModel vm)
        {
            var view = vm as CheckoutViewModel;
            //view.RowIntoView(PropertyChangedArgs.RowInfo.Sale, PropertyChangedArgs.RowInfo.Transactions);
        }

        public override async void Command(object parameter, ActionType intent)
        {
            var newValue = PropertyChangedArgs.NewValue as int? ?? 0;
            var oldValue = PropertyChangedArgs.OldValue as int? ?? 0;

            if (newValue == oldValue)
                return;

            var amount = Math.Abs(oldValue - newValue);

            var isIncrease = newValue > oldValue;

            var shouldAdd =
                intent == ActionType.Undo
                    ? !isIncrease
                    : isIncrease;

            if (shouldAdd)
            {
                var result = await _checkoutService.AddItem(_dto.SaleId, _dto.ProductId,amount);

                _transactionStore.AddToSale(result);

                _dto.TransactionId = result.Transaction.Id;
            }
            else
            {
                var result = await _checkoutService.Subtract(_dto.SaleId, _dto.TransactionId, amount);

                _transactionStore.AddToSale(result);
            }
        }

        public new class DTO: PropertyChangeCommand<TransactionVMRowInfo>.DTO
        {
            public int SaleId { get; set; }
            public int ProductId { get; set; }
            public int TransactionId { get; set; }
        }
    }

}
