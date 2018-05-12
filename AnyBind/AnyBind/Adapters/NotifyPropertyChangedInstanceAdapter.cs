using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AnyBind.Adapters
{
    public class NotifyPropertyChangedInstanceAdapter : IInstanceAdapter
    {
        private INotifyPropertyChanged Instance { get; set; }
        private bool IsSubscribed { get; set; } = false;
        private SortedSet<string> SubscribedProperties { get; } = new SortedSet<string>();

        public NotifyPropertyChangedInstanceAdapter(INotifyPropertyChanged instance)
        {
            Instance = instance;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            if (IsSubscribed)
            {
                Instance.PropertyChanged -= Instance_PropertyChanged;
            }
            Instance = null;
        }

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (SubscribedProperties.Contains(e.PropertyName))
                OnPropertyChanged(e.PropertyName);
        }

        public string[] SubscribeToProperties(params string[] propertyNames)
        {
            if (Instance == null)
                return new string[0];
            if (!IsSubscribed)
            {
                Instance.PropertyChanged += Instance_PropertyChanged;
                IsSubscribed = true;
            }
            foreach (var property in propertyNames)
            {
                SubscribedProperties.Add(property);
            }
            return propertyNames;
        }

        public void UnsubscribeFromProperties(params string[] propertyNames)
        {
            foreach (var property in propertyNames)
            {
                SubscribedProperties.Remove(property);
            }
            if (SubscribedProperties.Count == 0)
            {
                Instance.PropertyChanged -= Instance_PropertyChanged;
                IsSubscribed = false;
            }
        }
    }
}
