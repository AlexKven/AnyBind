using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace AnyBind.Adapters
{
    public class ObservableCollectionInstanceAdapter<T> : IInstanceAdapter
    {
        private ObservableCollection<T> Instance { get; set; }
        private bool IsSubscribed { get; set; }
        private SortedSet<string> SubscribedProperties { get; } = new SortedSet<string>();
        private SortedSet<int> SubscribedIndices { get; } = new SortedSet<int>();

        public ObservableCollectionInstanceAdapter(ObservableCollection<T> instance)
        {
            Instance = instance;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RaiseIfSubscribed(string property)
        {
            if (SubscribedProperties.Contains(property))
                OnPropertyChanged(property);
        }

        private void RaiseIfSubscribed(int index)
        {
            if (SubscribedIndices.Contains(index))
                OnPropertyChanged($"[{index}]");
        }

        public void Dispose()
        {
            if (IsSubscribed)
            {
                Instance.CollectionChanged -= Instance_CollectionChanged;
            }
            Instance = null;
        }

        public bool SubscribeToProperties(params string[] propertyName)
        {
            if (Instance == null)
                return false;
            if (!IsSubscribed)
            {
                Instance.CollectionChanged += Instance_CollectionChanged;
            }
            foreach (var property in propertyName)
            {
                if (property.StartsWith("[") && property.EndsWith("]")
                    && int.TryParse(property.Substring(1, property.Length - 2), out int index))
                {
                    SubscribedIndices.Add(index);
                }
                else
                {
                    SubscribedProperties.Add(property);
                }
            }
            return true;
        }

        private void Instance_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("[]");
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    RaiseIfSubscribed("Count");
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        var index = e.NewStartingIndex + i;
                        RaiseIfSubscribed(index);
                    }
                    break;
            }
        }

        public void UnsubscribeFromProperties(params string[] propertyName)
        {
            foreach (var property in propertyName)
            {
                if (property.StartsWith("[") && property.EndsWith("]")
                    && int.TryParse(property.Substring(1, property.Length - 2), out int index))
                {
                    SubscribedIndices.Remove(index);
                }
                else
                {
                    SubscribedProperties.Remove(property);
                }
            }
            if (SubscribedProperties.Count == 0)
            {
                Instance.CollectionChanged -= Instance_CollectionChanged;
                IsSubscribed = false;
            }
        }
    }
}
