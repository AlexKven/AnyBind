using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace AnyBind.Adapters
{
    public class ObservableCollectionInstanceAdapter<T> : IInstanceAdapter
    {
        private ObservableCollection<T> Instance { get; set; }
        private bool IsSubscribed { get; set; }
        private Dictionary<string, int> SubscribedProperties { get; } = new Dictionary<string, int>();
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
            if (SubscribedProperties.ContainsKey(property))
                OnPropertyChanged(property);
        }

        private void RaiseIfSubscribed(int index)
        {
            if (SubscribedIndices.Contains(index))
            {
                foreach (var prop in SubscribedProperties
                    .Where(kvp => kvp.Value == index)
                    .Select(kvp => kvp.Key))
                {
                    OnPropertyChanged(prop);
                }
            }
        }

        public void Dispose()
        {
            if (IsSubscribed)
            {
                Instance.CollectionChanged -= Instance_CollectionChanged;
            }
            Instance = null;
        }

        public string[] SubscribeToProperties(params string[] propertyNames)
        {
            List<string> exclude = new List<string>();
            if (Instance == null)
                return new string[0];
            if (!IsSubscribed)
            {
                Instance.CollectionChanged += Instance_CollectionChanged;
            }
            foreach (var property in propertyNames)
            {
                int index = -1;
                if (property.StartsWith("[") && property.EndsWith("]")
                    && int.TryParse(property.Substring(1, property.Length - 2), out index))
                {
                    SubscribedIndices.Add(index);
                }
                else
                {
                    if (property != "Count" && property != "[]")
                    {
                        exclude.Add(property);
                        continue;
                    }
                }
                SubscribedProperties.Add(property, index);
            }
            if (exclude.Count == 0)
                return propertyNames;
            else
                return propertyNames.Except(exclude).ToArray();
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

        public void UnsubscribeFromProperties(params string[] propertyNames)
        {
            foreach (var property in propertyNames)
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
