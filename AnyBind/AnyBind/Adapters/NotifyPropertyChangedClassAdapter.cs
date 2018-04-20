using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace AnyBind.Adapters
{
    public class NotifyPropertyChangedClassAdapter : IClassAdapter
    {
        public bool CanSubscribe(TypeInfo typeInfo)
        {
            if (typeof(INotifyPropertyChanged).GetTypeInfo().IsAssignableFrom(typeInfo))
                return true;
            return false;
        }

        public IInstanceAdapter CreateInstanceAdapter(object instance)
        {
            return new NotifyPropertyChangedInstanceAdapter(instance as INotifyPropertyChanged);
        }

        public IEnumerable<string> FilterSubscribableProperties(TypeInfo typeInfo, IEnumerable<string> properties)
        {
            if (typeof(INotifyPropertyChanged).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                foreach (var prop in properties)
                    yield return prop;
            }
        }
    }
}
