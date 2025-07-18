using Mhyrenz_Interface.Core;
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

        public enum ActionType
        {
            Normal, Undo, Redo
        }

        private ActionType Action { get; set; }

        public PropertyChangeCommand(T target, string propertyName, object oldValue, object newValue)
        {
            _target = target;
            _propertyName = propertyName;
            _oldValue = oldValue;
            _newValue = newValue;
        }
        public void Execute()
        {
            //SetProperty(_newValue);
            Action = ActionType.Normal;
            Command(_newValue, ActionType.Normal);
        }

        public void Undo()
        {
            SetProperty(_oldValue);

            Action = ActionType.Undo;

            Command(_oldValue, ActionType.Undo);
        }

        public void Redo()
        {
            SetProperty(_newValue);

            Action = ActionType.Redo;

            Command(_newValue, ActionType.Redo);
        }

        public abstract bool Command(object parameter, ActionType intent);

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
