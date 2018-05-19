using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace AnyBind.Caching
{
    public abstract class CacheBase<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        protected abstract bool TryGetValueInternal(TKey key, out TValue result);
        protected abstract bool TrySetValueInternal(TKey key, TValue value);
        protected abstract bool TryClearValueInternal(TKey key, out TValue result);

        public CacheBase()
        {
            _SetActions.CollectionChanged += _SetActions_CollectionChanged;
            _RemoveActions.CollectionChanged += _RemoveActions_CollectionChanged;
        }

        private void _SetActions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Move)
            {
                foreach (var item in this)
                {
                    foreach (Action<TKey, TValue> action in e.NewItems)
                    {
                        action(item.Key, item.Value);
                    }
                }
            }
        }

        private void _RemoveActions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Move)
            {
                foreach (var item in this)
                {
                    foreach (Action<TKey, TValue> action in e.OldItems)
                    {
                        action(item.Key, item.Value);
                    }
                }
            }
        }

        public bool TryGetValue(TKey key, out TValue result)
        {
            var success = TryGetValueInternal(key, out result);
            foreach (var action in GetActions)
            {
                action(key, result);
            }
            return success;
        }

        public bool TrySetValue(TKey key, TValue item)
        {
            if (SetActions.Count > 0 && TryGetValueInternal(key, out var result))
            {
                foreach (var action in RemoveActions)
                {
                    action(key, result);
                }
            }
            var success = TrySetValueInternal(key, item);
            if (success)
            {
                foreach (var action in SetActions)
                {
                    action(key, item);
                }
            }
            return success;
        }

        public bool TryClearValue(TKey key)
        {
            if(TryClearValueInternal(key, out var result))
            {
                foreach (var action in RemoveActions)
                {
                    action(key, result);
                }
                return true;
            }
            return false;
        }

        public abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public TValue this[TKey key]
        {
            get
            {
                if (TryGetValue(key, out var result))
                    return result;
                else
                    return default(TValue);
            }
            set
            {
                TrySetValue(key, value);
            }
        }

        private ObservableCollection<Action<TKey, TValue>> _SetActions = new ObservableCollection<Action<TKey, TValue>>();
        public IList<Action<TKey, TValue>> SetActions => _SetActions;

        private List<Action<TKey, TValue>> _GetActions = new List<Action<TKey, TValue>>();
        public IList<Action<TKey, TValue>> GetActions => _GetActions;

        private ObservableCollection<Action<TKey, TValue>> _RemoveActions = new ObservableCollection<Action<TKey, TValue>>();
        public IList<Action<TKey, TValue>> RemoveActions => _RemoveActions;

        public abstract IEnumerable<TKey> Keys { get; }

        public abstract bool ContainsKey(TKey key);
    }
}
