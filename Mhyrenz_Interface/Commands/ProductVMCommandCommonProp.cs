using System;
using System.Windows.Input;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.ViewModels;

namespace Mhyrenz_Interface.Commands
{
    public class ProductVMCommandCommonProp : ProductVMPropertyChangeCommand
    {
        private readonly ProductDataViewModel _target;
        private readonly ICommand _command;

        public ProductVMCommandCommonProp(ProductDataViewModel target,
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
            var product = _target.Item;

            _command.Execute(new UpdateProductCommand.DTO()
            {
                Id = product.Id,
                UpdatedProduct = product
            });

            return true;
        }
    }
}
