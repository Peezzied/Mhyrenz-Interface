using System;
using System.Windows.Input;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.ViewModels;

namespace Mhyrenz_Interface.Commands
{
    public class ProductVMCommandCommonProp : PropertyChangeCommand<ProductDataViewModel>
    {
        private readonly ProductDataViewModel _target;
        private readonly string _propertyName;
        private readonly object _oldValue;
        private readonly object _newValue;
        private readonly ICommand _command;

        public ProductVMCommandCommonProp(ProductDataViewModel target,
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
            _propertyName = propertyName;
            _command = command;
        }

        public override bool Command(object parameter, ActionType intent)
        {
            var product = _target.Item;
            product.GetType().GetProperty(_propertyName).SetValue(product, intent == ActionType.Undo ? _oldValue : _newValue);
            _command.Execute(new UpdateProductCommand.DTO()
            {
                Id = product.Id,
                UpdatedProduct = product
            });

            return true;
        }
    }
}
