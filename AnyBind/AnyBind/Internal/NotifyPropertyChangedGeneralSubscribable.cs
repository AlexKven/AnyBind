using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AnyBind.Internal
{
    internal class NotifyPropertyChangedGeneralSubscribable : GeneralSubscribableBase
    {
        private INotifyPropertyChanged Instance { get; }

        public NotifyPropertyChangedGeneralSubscribable(INotifyPropertyChanged instance) : base(instance)
        {
            Instance = instance;

            Instance.PropertyChanged += Instance_PropertyChanged;
        }

        public override event PropertyChangedEventHandler PropertyChanged;

        public override void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            Instance.RaiseEvent("PropertyChanged", e);
        }

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged(sender, e);
        }
    }
}
