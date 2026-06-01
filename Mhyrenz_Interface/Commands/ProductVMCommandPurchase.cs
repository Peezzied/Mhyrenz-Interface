using System;
using System.Windows.Input;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;

namespace Mhyrenz_Interface.Commands
{
    public class ProductVMCommandPurchase : ProductVMPropertyChangeCommand
    {
        private readonly int _target;
        private readonly ICommand _command;

        public ProductVMCommandPurchase(int target,
            ChangedArgs args,
            TrackPropertyHelper.Setter setter,
            ICommand command,
            Action propertyChangeHandler,
            Type currentViewIn) : base(args, setter, propertyChangeHandler, currentViewIn)
        {
            _target = target;
            _command = command;
        }

        public override bool Command(object parameter, ActionType intent)
        {
            var newValue = PropertyChangedArgs.NewValue as int? ?? 0;
            var oldValue = PropertyChangedArgs.OldValue as int? ?? 0;

            DirectPurchaseCommand.DTO.Type method;
            if (newValue > oldValue)
                method = intent == ActionType.Undo ? DirectPurchaseCommand.DTO.Type.Subtract : DirectPurchaseCommand.DTO.Type.Add;
            else if (newValue < oldValue)
                method = intent == ActionType.Undo ? DirectPurchaseCommand.DTO.Type.Add : DirectPurchaseCommand.DTO.Type.Subtract;
            else
                return false;

            _command.Execute(new DirectPurchaseCommand.DTO()
            {
                Amount = Math.Abs(oldValue - newValue),
                ProductId = _target,
                Method = method,
            });

            return true;
        }
    }
}
