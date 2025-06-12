using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Core
{
    public class TrackerManager<T> where T : BaseViewModel
    {
        private readonly Dictionary<T, List<TrackerRegistration<T>>> _trackerMap = new Dictionary<T, List<TrackerRegistration<T>>>();

        private class TrackerRegistration<TTarget> where TTarget : BaseViewModel
        {
            public PropertyChangeTracker<TTarget> Tracker { get; }
            public EventHandler<TargetChangedEventArgs> Handler { get; }

            public TrackerRegistration(PropertyChangeTracker<TTarget> tracker, EventHandler<TargetChangedEventArgs> handler)
            {
                Tracker = tracker;
                Handler = handler;
            }
        }

        public void Track(T target, params (PropertyChangeTracker<T> tracker, EventHandler<TargetChangedEventArgs> handler)[] registrations)
        {
            Untrack(target);

            var list = new List<TrackerRegistration<T>>();

            foreach (var (tracker, handler) in registrations)
            {
                tracker.PropertyChanged += handler;
                list.Add(new TrackerRegistration<T>(tracker, handler));
            }

            _trackerMap[target] = list;
        }

        public void Untrack(T target)
        {
            if (_trackerMap.TryGetValue(target, out var list))
            {
                foreach (var reg in list)
                {
                    reg.Tracker.PropertyChanged -= reg.Handler;
                }

                _trackerMap.Remove(target);
            }
        }

        public void UntrackAll()
        {
            foreach (var pair in _trackerMap)
            {
                foreach (var reg in pair.Value)
                {
                    reg.Tracker.PropertyChanged -= reg.Handler;
                }
            }

            _trackerMap.Clear();
        }
    }

}
