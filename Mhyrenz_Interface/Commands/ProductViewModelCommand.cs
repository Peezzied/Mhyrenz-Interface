using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Input;

namespace Mhyrenz_Interface.Commands
{
    public class ProductViewModelCommand : PropertyChangeCommand<ProductViewModel>
    {
        private readonly ProductViewModel _target;
        private readonly string _propertyName;
        private readonly object _oldValue;
        private readonly object _newValue;
        private readonly ICommand _command;

        public ProductViewModelCommand(
            ProductViewModel target,
            string propertyName,
            object oldValue,
            object newValue,
            ICommand command) : base(target, propertyName, oldValue, newValue)
        {
            _target = target;
            _propertyName = propertyName;
            _oldValue = oldValue;
            _newValue = newValue;
            _command = command;
        }

        public override bool Command(object parameter)
        {
            _command.Execute(new UpdateProductCommandDTO()
            {
                Id = _target.Item.Id,
                PropertyName = _propertyName,
                Value = parameter
            });

            return true;
        }

        //private Product ProductReflect(object parameter)
        //{
        //    var product = _target.Item.Clone();

        //    // Get the property's name being tracked
        //    var propertyName = this._propertyName;  // e.g., "CategoryId"

        //    // Get the PropertyInfo for the Product class
        //    var productProperty = typeof(Product).GetProperty(propertyName);
        //    if (productProperty == null)
        //        throw new InvalidOperationException($"Property '{propertyName}' does not exist on Product.");

        //    // Convert parameter to the property type
        //    var convertedValue = Convert.ChangeType(parameter, productProperty.PropertyType);

        //    // Set the property value
        //    productProperty.SetValue(product, convertedValue);

        //    return product;
        //}
    }
}
