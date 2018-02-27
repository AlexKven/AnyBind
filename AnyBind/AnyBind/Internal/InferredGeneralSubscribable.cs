using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AnyBind.Internal
{
    internal class InferredGeneralSubscribable : GeneralSubscribableBase
    {
        public InferredGeneralSubscribable(object instance) : base(instance)
        {
        }

        public override event PropertyChangedEventHandler PropertyChanged;

        public override void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
        }
    }
}
