using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AnyBind.Adapters
{
    public interface IInstanceAdapter : IDisposable, INotifyPropertyChanged
    {
        string[] SubscribeToProperties(params string[] propertyNames);
        void UnsubscribeFromProperties(params string[] propertyNames);
    }
}
