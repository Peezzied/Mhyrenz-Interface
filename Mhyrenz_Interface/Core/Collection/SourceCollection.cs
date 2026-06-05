using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;

namespace Mhyrenz_Interface.Core.Collection
{
    public sealed class SourceCollection<TKey, TValue> : IEnumerable<TValue>, IReadOnlyCollection<TValue>, IDisposable
    {
        private readonly Func<TValue, TKey> _keySelector;

        public ObservableList<TValue> Source { get; } =
            new ObservableList<TValue>();

        public Dictionary<TKey, TValue> Lookup { get; } =
            new Dictionary<TKey, TValue>();

        public int Count => Source.Count;

        public IEnumerable<TKey> Keys => Lookup.Keys;

        public IEnumerable<TValue> Values => Lookup.Values;

        public TValue this[TKey key] => Lookup[key];

        public SourceCollection(Func<TValue, TKey> keySelector)
        {
            _keySelector = keySelector;
        }

        public void Add(TValue item)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var key = _keySelector(item);

                Lookup[key] = item;
                Source.Add(item);
            });
        }

        public void AddRange(IEnumerable<TValue> items)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var list = items.ToList();

                foreach (var item in list)
                {
                    Lookup[_keySelector(item)] = item;
                    Source.Add(item);
                }
            });
        }

        public bool Remove(TKey key)
        {
            return RemoveMany(new[] { key }) > 0;
        }

        public int RemoveMany(IEnumerable<TKey> keys)
        {
            return App.Current.Dispatcher.Invoke(() =>
            {
                var keySet = new HashSet<TKey>(keys);

                var indexes = Source
                    .Select((item, index) => new
                    {
                        Item = item,
                        Index = index
                    })
                    .Where(x => keySet.Contains(_keySelector(x.Item)))
                    .OrderByDescending(x => x.Index)
                    .ToList();

                foreach (var entry in indexes)
                {
                    Source.RemoveAt(entry.Index);
                    Lookup.Remove(_keySelector(entry.Item));
                }

                return indexes.Count;
            });
        }

        public void Clear()
        {
            Lookup.Clear();
            Source.Clear();
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return Source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            // TODO dispose the source collection
            throw new NotImplementedException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return Lookup.TryGetValue(key, out value);
        }
    }
}
