using System;
using System.Runtime.CompilerServices;
using Mhyrenz_Interface.Core.MVVM;

namespace Mhyrenz_Interface.Core.PropertyTracking
{
    public class TrackedViewModel : BaseViewModel, ITrackPropertyChanged
    {
        public PropertyChangeOrigin TrackingOrigin { get; set; } = PropertyChangeOrigin.User;

        public event TrackedPropertyChangedHandler TrackedPropertyChanged;

        protected virtual void OnTrackedPropertyChanged<T>(T newValue, T oldValue, [CallerMemberName] string propertyName = null)
        {
            TrackedPropertyChanged?.Invoke(this, new TrackedPropertyChangedEventArgs(newValue, oldValue, propertyName, TrackingOrigin));
            base.OnPropertyChanged(propertyName);
        }

        protected void SetTrackedProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            var prev = storage;
            storage = value;
            OnTrackedPropertyChanged(value, prev, propertyName);
        }

        protected void SetTrackedProperty<T>(T prev, T value, Action<T> setter, [CallerMemberName] string propertyName = null)
        {
            setter(value);
            OnTrackedPropertyChanged(value, prev, propertyName);
        }

        protected void DeferSetTrackedProperty<T>(T prev, T value, [CallerMemberName] string propertyName = null)
        {
            TrackedPropertyChanged?.Invoke(this, new TrackedPropertyChangedEventArgs(value, prev, propertyName, TrackingOrigin));
        }

        public virtual void SetValue(string propertyName, object value)
        {
            base.OnPropertyChanged(propertyName);
        }
    }
}
