using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Core
{
    public class SmartObservableCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotification = false;

        public SmartObservableCollection() : base() { }

        public SmartObservableCollection(IEnumerable<T> collection) : base(collection) { }

        /// <summary>
        /// Returns a disposable that suppresses notifications until disposed
        /// </summary>
        public IDisposable SuppressNotifications()
        {
            return new NotificationSuppressor(this);
        }

        private class NotificationSuppressor : IDisposable
        {
            private SmartObservableCollection<T> _collection;

            public NotificationSuppressor(SmartObservableCollection<T> collection)
            {
                _collection = collection;
                _collection._suppressNotification = true;
            }

            public void Dispose()
            {
                _collection._suppressNotification = false;
                _collection.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                _collection = null;
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
            {
                base.OnCollectionChanged(e);
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (!_suppressNotification)
            {
                base.OnPropertyChanged(e);
            }
        }
    }

}
