using System;
using System.Collections.Generic;
using System.Text;

namespace AnyBind.Caching
{
    public class StrongReferenceCache<TKey, TValue> : CacheBase<TKey, TValue>
    {
        private Dictionary<TKey, TValue> DataStore { get; } = new Dictionary<TKey, TValue>();

        public override IEnumerable<TKey> Keys => DataStore.Keys;

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => DataStore.GetEnumerator();

        protected override bool TryClearValueInternal(TKey key, out TValue result)
        {
            if (DataStore.TryGetValue(key, out result))
            {
                DataStore.Remove(key);
                return true;
            }
            result = default(TValue);
            return false;
        }

        protected override bool TryGetValueInternal(TKey key, out TValue result)
        {
            if (DataStore.TryGetValue(key, out result))
            {
                return true;
            }
            result = default(TValue);
            return false;
        }

        protected override bool TrySetValueInternal(TKey key, TValue value)
        {
            DataStore[key] = value;
            return true;
        }

        public override bool ContainsKey(TKey key) => DataStore.ContainsKey(key);
    }
}
