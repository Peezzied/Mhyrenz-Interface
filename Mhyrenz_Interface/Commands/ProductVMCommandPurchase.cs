using MahApps.Metro.Controls;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Mhyrenz_Interface.Commands
{

    public class PurchaseProductDTO
    {
        public enum Type { Add, Remove }

        public int Amount { get; set; }
        public Product Product { get; set; }
        public Type Method { get; set; }
        public PropertyChangeCommand<ProductDataViewModel>.ActionType Intent { get; internal set; }
    }

    public class ProductVMCommandPurchase : PropertyChangeCommand<ProductDataViewModel>
    {
        private readonly ProductDataViewModel _target;
        private readonly string _propertyName;
        private readonly object _oldValue;
        private readonly object _newValue;
        private readonly ICommand _command;
        private readonly Action _propertyChangeHandler;

        public ProductVMCommandPurchase(
            ProductDataViewModel target,
            string propertyName,
            object oldValue,
            object newValue,
            ICommand command,
            Action propertyChangeHandler) : base(target, propertyName, oldValue, newValue)
        {
            _target = target;
            _propertyName = propertyName;
            _oldValue = oldValue;
            _newValue = newValue;
            _command = command;
            _propertyChangeHandler = propertyChangeHandler;
        }

        public override bool Command(object parameter, ActionType intent)
        {
            var amount = parameter as int? ?? 0;
            var newValue = _newValue as int? ?? 0;
            var oldValue = _oldValue as int? ?? 0;

            PurchaseProductDTO.Type? method;
            if (newValue > oldValue)
                method = PurchaseProductDTO.Type.Add;
            else if (newValue < oldValue)
                method = PurchaseProductDTO.Type.Remove;
            else
                return false;

            _command.Execute(new PurchaseProductDTO()
            {
                Amount = Math.Abs(oldValue - amount),
                Product = _target.Item,
                Method = method.Value,
                Intent = intent
            });

            _propertyChangeHandler();

            return true;
        }
    }
}
