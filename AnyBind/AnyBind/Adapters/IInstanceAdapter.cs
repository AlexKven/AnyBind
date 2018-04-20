using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AnyBind.Adapters
{
    public interface IInstanceAdapter : IDisposable, INotifyPropertyChanged
    {
        bool SubscribeToProperties(params string[] propertyName);
        void UnsubscribeFromProperties(params string[] propertyName);
    }
}
