using System;
using Mhyrenz_Interface.Store;
using static Mhyrenz_Interface.Core.PropertyTracking.TrackPropertyHelper;

namespace Mhyrenz_Interface.Core.PropertyTracking
{
    public static class TrackPropertyHelper
    {
        public delegate void Setter(object value, PropertyChangeOrigin origin = PropertyChangeOrigin.Programmatic);
        [Obsolete]
        public delegate object Getter();

        public static TrackPropertyHelper<TKey, TValue> Build<TKey, TValue>(IViewModelStore<TKey, TValue> store, TKey key, string propertyName)
            where TValue : TrackedViewModel
        {
            return new TrackPropertyHelper<TKey, TValue>(store, key, propertyName);
        }
    }

    public class TrackPropertyHelper<TKey, TValue> where TValue : TrackedViewModel
    {
        private readonly IViewModelStore<TKey, TValue> _store;
        private readonly TKey _key;
        private readonly TrackedViewModel _obj;
        private readonly string _propertyName;

        public TrackPropertyHelper(IViewModelStore<TKey, TValue> store, TKey key, string propertyName)
        {
            _store = store;
            _key = key;
            _propertyName = propertyName;
        }

        public delegate void Handler(Setter setter, TKey key);

        public TrackPropertyHelper<TKey, TValue> Track(string propertyName, Handler handler, Setter setter = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException($"'{nameof(propertyName)}' cannot be null or empty.", nameof(propertyName));
            }

            if (_propertyName != propertyName)
                return this;

            if (setter == null)
            {
                setter = (object val, PropertyChangeOrigin origin) =>
                {
                    if (!_store.Store.TryGetValue(_key, out var vm))
                        return;

                    var property = vm.GetType().GetProperty(propertyName) ?? throw new InvalidOperationException(
                            $"Property '{propertyName}' was not found on '{typeof(TValue).Name}'.");
                    vm.TrackingOrigin = origin;

                    try
                    {
                        property.SetValue(vm, val);
                    }
                    finally
                    {
                        vm.TrackingOrigin = default;
                    }
                };
            }

            handler(setter, _key);
            return this;
        }
    }
}
