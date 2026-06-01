using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
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

    public class TransactionVMCommandProp : PropertyChangeCommand<TransactionVMRowInfo>
    {
        private readonly ICommand _command;
        private int _productId;
        private readonly int _saleId;

        public TransactionVMCommandProp(int saleId, int productId, ChangedArgs args, TrackPropertyHelper.Setter setter, ICommand command, Action propertyChangeHandler, Type currentViewIn) : 
            base(args, setter, propertyChangeHandler, currentViewIn)
        {
            _productId = productId;
            _saleId = saleId;
            _command = command;
            SideEffect = SideEffectHandler;
        }

        private void SideEffectHandler(NavigationViewModel vm)
        {
            var view = vm as CheckoutViewModel;
            //view.RowIntoView(PropertyChangedArgs.RowInfo.Sale, PropertyChangedArgs.RowInfo.Transactions);
        }

        public override bool Command(object parameter, ActionType intent)
        {
            var newValue = PropertyChangedArgs.NewValue as int? ?? 0;
            var oldValue = PropertyChangedArgs.OldValue as int? ?? 0;

            SaleBoundPurchaseCommand.DTO.Type method;
            if (newValue > oldValue)
                method = intent == ActionType.Undo ? SaleBoundPurchaseCommand.DTO.Type.Subtract : SaleBoundPurchaseCommand.DTO.Type.Add;
            else if (newValue < oldValue)
                method = intent == ActionType.Undo ? SaleBoundPurchaseCommand.DTO.Type.Add : SaleBoundPurchaseCommand.DTO.Type.Subtract;
            else
                return false;

            _command.Execute(new SaleBoundPurchaseCommand.DTO()
            {
                Amount = Math.Abs(oldValue - newValue),
                SaleId = _saleId,
                ProductId = _productId,
                Method = method,
            });

            return true;
        }
    }

}
