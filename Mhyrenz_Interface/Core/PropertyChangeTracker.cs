using Mhyrenz_Interface.State;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Core
{
    public class TargetChangedEventArgs : EventArgs
    {
        public object Target { get; }
        public string PropertyOf { get; set; }

        public TargetChangedEventArgs(object target, string propertyOf)
        {
            Target = target;
            PropertyOf = propertyOf;
        }
    }


    public class PropertyChangeTracker<T> where T : BaseViewModel
    {
        public Dictionary<string, object> PreviousValues { get; } = new Dictionary<string, object>();
        private readonly Action<string, object, object> _onPropertyChanged;

        public event EventHandler<TargetChangedEventArgs> PropertyChanged;

        public PropertyChangeTracker(T target, Action<string, object, object> onPropertyChanged)
        {
            _onPropertyChanged = onPropertyChanged;
            target.PropertyChanged += HandlePropertyChanged;
        }

        public PropertyChangeTracker<T> Track(string propertyName, object value)
        {
            PreviousValues[propertyName] = value;
            return this;
        }

        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ( ChangeTracking.Suppress ) return;
            if (!(sender is T target)) return;

            var propertyName = e.PropertyName;

            var type = typeof(T);
            var property = type.GetProperty(propertyName);
            if (property == null) return;

            var newValue = property.GetValue(target);
            if (PreviousValues.TryGetValue(propertyName, out var oldValue))
            {
                _onPropertyChanged?.Invoke(propertyName, oldValue, newValue);
            }
            else 
                return;

            PropertyChanged?.Invoke(this, new TargetChangedEventArgs(sender, propertyName));

            // Update the last known value
            PreviousValues[propertyName] = newValue;
        }
    }

}
