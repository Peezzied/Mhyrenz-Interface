using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Mhyrenz_Interface.Core
{
    public class ObservableDictionary<TKey, TValue> :
    IDictionary<TKey, TValue>,
    INotifyCollectionChanged,
    INotifyPropertyChanged
    {
        private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<ValueChangedEventArgs<TKey, TValue>> ValueChanged;

        // Keep track of subscribed PropertyChanged handlers
        private readonly Dictionary<TKey, PropertyChangedEventHandler> _valueSubscriptions =
            new Dictionary<TKey, PropertyChangedEventHandler>();

        // ----------------- Constructors -----------------

        public ObservableDictionary()
        {
        }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) throw new ArgumentNullException("dictionary");

            foreach (var kvp in dictionary)
            {
                Add(kvp.Key, kvp.Value);
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        private void OnValueChanged(TKey key, TValue oldValue, TValue newValue)
        {
            ValueChanged?.Invoke(this, new ValueChangedEventArgs<TKey, TValue>(key, oldValue, newValue));
        }

        private void SubscribeValue(TKey key, TValue value)
        {
            if (value is INotifyPropertyChanged notify)
            {
                void handler(object s, PropertyChangedEventArgs e)
                {
                    OnValueChanged(key, value, value);
                }
                notify.PropertyChanged += handler;
                _valueSubscriptions[key] = handler;
            }
        }

        private void UnsubscribeValue(TKey key, TValue value)
        {
            if (value is INotifyPropertyChanged notify && _valueSubscriptions.ContainsKey(key))
            {
                notify.PropertyChanged -= _valueSubscriptions[key];
                _valueSubscriptions.Remove(key);
            }
        }

        // ----------------- Indexer -----------------

        public TValue this[TKey key]
        {
            get
            {
                _dictionary.TryGetValue(key, out TValue value);
                return value; // returns default(TValue) instead of throwing
            }
            set
            {
                bool exists = _dictionary.TryGetValue(key, out TValue oldValue);

                if (exists)
                {
                    // Unsubscribe old value if needed
                    UnsubscribeValue(key, oldValue);
                }

                _dictionary[key] = value;

                if (exists)
                {
                    // Fire Replace event
                    OnCollectionChanged(
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Replace,
                            new KeyValuePair<TKey, TValue>(key, value),
                            new KeyValuePair<TKey, TValue>(key, oldValue)
                        )
                    );

                    // Fire ValueChanged if different
                    if (!object.Equals(oldValue, value))
                        OnValueChanged(key, oldValue, value);
                }
                else
                {
                    // Fire Add event
                    OnCollectionChanged(
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Add,
                            new KeyValuePair<TKey, TValue>(key, value)
                        )
                    );
                }

                OnPropertyChanged(nameof(Count));

                // Subscribe to new value if it implements INotifyPropertyChanged
                SubscribeValue(key, value);
            }
        }

        // ----------------- Properties -----------------

        public ICollection<TKey> Keys { get { return _dictionary.Keys; } }
        public ICollection<TValue> Values { get { return _dictionary.Values; } }
        public int Count { get { return _dictionary.Count; } }
        public bool IsReadOnly { get { return false; } }

        // ----------------- Add / Remove -----------------

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);

            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    new KeyValuePair<TKey, TValue>(key, value)
                )
            );

            OnPropertyChanged(nameof(Count));

            SubscribeValue(key, value);
        }

        public bool Remove(TKey key)
        {
            if (_dictionary.TryGetValue(key, out TValue value))
            {
                _dictionary.Remove(key);
                UnsubscribeValue(key, value);

                OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Reset));

                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(nameof(Keys));
                OnPropertyChanged(nameof(Values));

                return true;
            }

            return false;
        }

        public void Clear()
        {
            foreach (var key in new List<TKey>(_dictionary.Keys))
            {
                UnsubscribeValue(key, _dictionary[key]);
            }

            _dictionary.Clear();

            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
            );

            OnPropertyChanged(nameof(Count));
        }

        // ----------------- Queries -----------------

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        // ----------------- IDictionary Helpers -----------------

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary<TKey, TValue>)_dictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }
    }

    // ----------------- EventArgs -----------------

    public class ValueChangedEventArgs<TKey, TValue> : EventArgs
    {
        public TKey Key { get; private set; }
        public TValue OldValue { get; private set; }
        public TValue NewValue { get; private set; }

        public ValueChangedEventArgs(TKey key, TValue oldValue, TValue newValue)
        {
            Key = key;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }


}
