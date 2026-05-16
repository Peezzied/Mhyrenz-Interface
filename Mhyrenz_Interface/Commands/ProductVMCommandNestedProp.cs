using System;
using System.Collections.Generic;
using System.Windows.Input;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.ViewModels;

namespace Mhyrenz_Interface.Commands
{
    public class ProductVMCommandNestedProp : PropertyChangeCommand<BaseViewModel>
    {
        private readonly ProductDataViewModel _targetProduct;
        private readonly BaseViewModel _owner;
        private readonly string _propertyName;
        private readonly object _oldValue;
        private readonly object _newValue;
        private readonly ICommand _command;

        [Obsolete]
        public ProductVMCommandNestedProp(BaseViewModel owner,
            string navigatorName,
            string propertyName,
            object oldValue,
            object newValue,
            ICommand command,
            Action propertyChangeHandler,
            Type currentViewIn,
            ProductDataViewModel targetProduct) : base(owner, navigatorName, oldValue, newValue, propertyChangeHandler, currentViewIn)
        {
            _targetProduct = targetProduct;
            _owner = owner;
            _oldValue = oldValue;
            _newValue = newValue;
            _propertyName = propertyName;
            _command = command;
        }

        public override bool Command(object parameter, ActionType intent)
        {
            //_command.Execute(new UpdateProductCommandDTO()
            //{
            //    Id = _targetProduct.Item.Id,
            //    Updater = entity =>
            //    {
            //        var value = intent == ActionType.Undo ? _oldValue : _newValue;

            //        if (entity.Extras is null)
            //            entity.Extras = new Dictionary<string, object>();

            //        entity.Extras[_propertyName] = value;
            //    }
            //});

            return true;
        }
    }
}
