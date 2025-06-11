using Mhyrenz_Interface.Domain.DTO;
using Mhyrenz_Interface.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mhyrenz_Interface.Commands
{
    public abstract class PropertyChangeCommand<T> : IUndoableCommand
    {
        private readonly T _target;
        private readonly string _propertyName;
        private readonly object _oldValue;
        private readonly object _newValue;

        public PropertyChangeCommand(T target, string propertyName, object oldValue, object newValue)
        {
            _target = target;
            _propertyName = propertyName;
            _oldValue = oldValue;
            _newValue = newValue;
        }
        public void Execute()
        {
            SetProperty(_newValue);
            
            Command(_newValue);
        }

        public void Undo()
        {
            SetProperty(_oldValue);
            Command(_oldValue);
        }

        public void Redo()
        {
            SetProperty(_newValue);
            Command(_newValue);
        }

        public abstract bool Command(object parameter);

        private void SetProperty(object value)
        {
            var prop = typeof(T).GetProperty(_propertyName);
            if (prop != null && prop.CanWrite)
            {
                ChangeTracking.Suppress = true;
                prop.SetValue(_target, value);
                ChangeTracking.Suppress = false;
            }
        }

    }

}
