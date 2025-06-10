using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Core
{
    public class PropertyChangeTracker<T> where T : BaseViewModel
    {
        private readonly Dictionary<string, object> _previousValues = new Dictionary<string, object>();
        private readonly Action<string, object, object> _onPropertyChanged;

        public PropertyChangeTracker(T target, Action<string, object, object> onPropertyChanged)
        {
            _onPropertyChanged = onPropertyChanged;
            target.PropertyChanged += HandlePropertyChanged;
        }

        public void Track(string propertyName, object value)
        {
            _previousValues[propertyName] = value;
        }

        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is T target)) return;

            var propertyName = e.PropertyName;

            var type = typeof(T);
            var property = type.GetProperty(propertyName);
            if (property == null) return;

            var newValue = property.GetValue(target);
            if (_previousValues.TryGetValue(propertyName, out var oldValue))
            {
                _onPropertyChanged?.Invoke(propertyName, oldValue, newValue);
            }

            // Update the last known value
            _previousValues[propertyName] = newValue;
        }
    }

}
