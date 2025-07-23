using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Input;

namespace Mhyrenz_Interface.Commands
{
    public class ProductVMCommandCommon : PropertyChangeCommand<ProductDataViewModel>
    {
        private readonly ProductDataViewModel _target;
        private readonly string _propertyName;
        private readonly object _oldValue;
        private readonly object _newValue;
        private readonly ICommand _command;

        public ProductVMCommandCommon(ProductDataViewModel target,
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
            _command.Execute(new UpdateProductCommandDTO()
            {
                Id = _target.Item.Id,
                PropertyName = _propertyName,
                Value = intent == ActionType.Undo ? _oldValue : _newValue
            });

            return true;
        }
    }
}
