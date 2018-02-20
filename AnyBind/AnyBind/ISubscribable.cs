using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AnyBind
{
    public interface ISubscribable : INotifyPropertyChanged
    {
        IEnumerable<string> SubscribableProperties { get; }

        void RaisePropertyChanged(PropertyChangedEventArgs e);
    }
}
