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
        public Dictionary<string, Action<PropertyChangeTracker<T>, TargetChangedEventArgs, object, object>> Methods { get; } = new Dictionary<string, Action<PropertyChangeTracker<T>, TargetChangedEventArgs, object, object>>();

        public PropertyChangeTracker(T target)
        {

            target.PropertyChanged += HandlePropertyChanged;
        }

        public PropertyChangeTracker<T> Track(string propertyName, object value, Action<PropertyChangeTracker<T>, TargetChangedEventArgs, object, object> onPropertyChanged)
        {
            PreviousValues[propertyName] = value;
            Methods[propertyName] = onPropertyChanged;
            return this;
        }

        public PropertyChangeTracker<T> Untrack()
        {
            PreviousValues.Clear();
            Methods.Clear();
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
                Methods[propertyName]?.Invoke(this, new TargetChangedEventArgs(
                    sender,
                    propertyName), oldValue, newValue);
            }
            else 
                return;

            // Update the last known value
            PreviousValues[propertyName] = newValue;
        }
    }

}
