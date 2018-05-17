using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AnyBind
{
    public interface ISubscribable : INotifyPropertyChanged
    {
        object GetPropertyValue(string propertyPath);

        void RaisePropertyChanged(PropertyChangedEventArgs e);

        void SubscribeToIndexedProperty(string index, string subscriberId);
        void UnsubscribeFromIndexedProperty(string index, string subscriberId);

        Type GetSubscribableType();
    }
}
