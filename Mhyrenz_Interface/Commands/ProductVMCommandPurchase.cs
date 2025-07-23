using MahApps.Metro.Controls;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
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
        public ActionType Intent { get; internal set; }
    }

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

            PurchaseProductDTO.Type? method;
            if (newValue > oldValue)
                method = intent == ActionType.Undo ? PurchaseProductDTO.Type.Remove : PurchaseProductDTO.Type.Add;
            else if (newValue < oldValue)
                method = intent == ActionType.Undo ? PurchaseProductDTO.Type.Add : PurchaseProductDTO.Type.Remove;
            else
                return false;

            _command.Execute(new PurchaseProductDTO()
            {
                Amount = Math.Abs(oldValue - newValue),
                Product = _target.Item,
                Method = method.Value,
            });

            return true;
        }
    }
}
