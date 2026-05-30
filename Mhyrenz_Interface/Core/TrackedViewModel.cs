using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Core
{
    public class TrackedViewModel : BaseViewModel, ITrackPropertyChanged
    {
        public PropertyChangeOrigin TrackingOrigin { get; set; } = PropertyChangeOrigin.User;

        public event TrackedPropertyChangedHandler TrackedPropertyChanged;

        protected virtual void OnTrackedPropertyChanged<T>(T oldValue, [CallerMemberName] string propertyName = null)
        {
            TrackedPropertyChanged?.Invoke(this, new TrackedPropertyChangedEventArgs(oldValue, propertyName, TrackingOrigin));
            base.OnPropertyChanged(propertyName);
        }

        protected void SetTrackedProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            var prev = storage;
            storage = value;
            OnTrackedPropertyChanged(prev, propertyName);
        }

        protected void SetTrackedProperty<T>(T prev, T value, Action<T> setter, [CallerMemberName] string propertyName = null)
        {
            setter(value);
            OnTrackedPropertyChanged(prev, propertyName);
        }
    }
}
