using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AnyBind.Internal
{
    internal class NotifyPropertyChangedGeneralSubscribable : GeneralSubscribableBase
    {
        public NotifyPropertyChangedGeneralSubscribable(INotifyPropertyChanged instance) : base(instance)
        {
            instance.PropertyChanged += Instance_PropertyChanged;
        }

        public override event PropertyChangedEventHandler PropertyChanged;

        public override void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
