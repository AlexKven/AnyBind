using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace AnyBind.Caching
{
    public class StrongObservableMultiCache<TKey, TValue> : StrongReferenceCache<TKey, ObservableCollection<TValue>>
    {
        protected override bool TrySetValueInternal(TKey key, ObservableCollection<TValue> value)
        {
            if (base.TrySetValueInternal(key, value))
            {
                value.CollectionChanged += Value_CollectionChanged;
                return true;
            }
            return false;
        }

        private void Value_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private ObservableCollection<Action<TKey, TValue>> _SetActions = new ObservableCollection<Action<TKey, TValue>>();
        public IList<Action<TKey, TValue>> SetSubItemActions => _SetActions;

        private ObservableCollection<Action<TKey, TValue>> _RemoveActions = new ObservableCollection<Action<TKey, TValue>>();
        public IList<Action<TKey, TValue>> RemoveSubItemActions => _RemoveActions;
    }
}
