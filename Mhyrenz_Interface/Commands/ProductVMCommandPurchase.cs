using System;
using System.Windows.Input;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.ViewModels;

namespace Mhyrenz_Interface.Commands
{
    public class ProductVMCommandPurchase : PropertyChangeCommand<ProductDataViewModel>
    {
        private readonly ProductDataViewModel _target;
        private readonly object _oldValue;
        private readonly object _newValue;
        private readonly ICommand _command;

        public ProductVMCommandPurchase(ProductDataViewModel target,
            string propertyName,
            object oldValue,
            object newValue,
            ICommand command,
            Action propertyChangeHandler,
            Type currentViewIn) : base(target, propertyName, oldValue, newValue, propertyChangeHandler, currentViewIn)
        {
            _target = target;
            _oldValue = oldValue;
            _newValue = newValue;
            _command = command;
        }

        public override bool Command(object parameter, ActionType intent)
        {
            var newValue = _newValue as int? ?? 0;
            var oldValue = _oldValue as int? ?? 0;

            PurchaseProductCommand.DTO.Type? method;
            if (newValue > oldValue)
                method = intent == ActionType.Undo ? PurchaseProductCommand.DTO.Type.Subtract : PurchaseProductCommand.DTO.Type.Add;
            else if (newValue < oldValue)
                method = intent == ActionType.Undo ? PurchaseProductCommand.DTO.Type.Add : PurchaseProductCommand.DTO.Type.Subtract;
            else
                return false;

            _command.Execute(new PurchaseProductCommand.DTO()
            {
                Amount = Math.Abs(oldValue - newValue),
                Product = _target.Item,
                Method = method.Value,
            });

            return true;
        }
    }
}
