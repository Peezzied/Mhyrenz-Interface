using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Commands
{
    public class PropertyChangeCommand<T> : IUndoableCommand
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
            SetProperty(_oldValue); // This line is not necessary, but it can be used to ensure the property is set to the new value first.
        }

        public void Undo()
        {
            SetProperty(_oldValue);
        }

        public void Redo()
        {
            SetProperty(_newValue);
        }

        private void SetProperty(object value)
        {
            var prop = typeof(T).GetProperty(_propertyName);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(_target, value);
            }
        }

    }

}
