using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AnyBind.Caching
{
    public class WeakReferenceCache<TKey, TValue> : CacheBase<TKey, TValue> where TValue : class
    {
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator, IDisposable
        {
            private Dictionary<TKey, WeakReference<TValue>>.Enumerator BaseEnumerator;
            private TValue CurrentValue;

            public Enumerator(Dictionary<TKey, WeakReference<TValue>>.Enumerator baseEnumerator)
            {
                BaseEnumerator = baseEnumerator;
                CurrentValue = default(TValue);
            }

            public object Current => new KeyValuePair<TKey, TValue>(BaseEnumerator.Current.Key, CurrentValue);

            KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current =>
                new KeyValuePair<TKey, TValue>(BaseEnumerator.Current.Key, CurrentValue);

            public void Dispose()
            {
                BaseEnumerator.Dispose();
            }

            public bool MoveNext()
            {
                bool result;
                do
                {
                    result = BaseEnumerator.MoveNext();
                }
                while (result && !BaseEnumerator.Current.Value.TryGetTarget(out CurrentValue));
                return result;
            }

            public void Reset()
            {
                ((IEnumerator)BaseEnumerator).Reset();
            }
        }

        private Dictionary<TKey, WeakReference<TValue>> DataStore { get; } = new Dictionary<TKey, WeakReference<TValue>>();

        public override IEnumerable<TKey> Keys => DataStore.Keys;

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new Enumerator(DataStore.GetEnumerator());

        protected override bool TryClearValueInternal(TKey key, out TValue result)
        {
            if (DataStore.TryGetValue(key, out var reference))
            {
                if (reference.TryGetTarget(out result))
                {
                    DataStore.Remove(key);
                    return true;
                }
            }
            result = default(TValue);
            return false;
        }

        protected override bool TryGetValueInternal(TKey key, out TValue result)
        {
            if (DataStore.TryGetValue(key, out var reference))
            {
                if (reference.TryGetTarget(out result))
                    return true;
            }
            result = default(TValue);
            return false;
        }

        protected override bool TrySetValueInternal(TKey key, TValue value)
        {
            DataStore[key] = new WeakReference<TValue>(value);
            return true;
        }

        public override bool ContainsKey(TKey key) => DataStore.ContainsKey(key);
    }
}
