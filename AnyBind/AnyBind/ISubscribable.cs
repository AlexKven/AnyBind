using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AnyBind
{
    public interface ISubscribable : INotifyPropertyChanged
    {
        IEnumerable<string> SubscribableProperties { get; }

        object GetPropertyValue(string propertyName);

        void RaisePropertyChanged(PropertyChangedEventArgs e);
    }
}
