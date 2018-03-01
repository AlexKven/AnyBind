using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AnyBind.Internal
{
    internal class GeneralSubscribable : ISubscribable
    {
        private object Instance { get; }
        private Type InstanceType { get; }
        private TypeInfo InstanceTypeInfo { get; }

        private INotifyPropertyChanged NotifyPropertyChanged;

        public GeneralSubscribable(object instance)
        {
            Instance = instance;
            InstanceType = instance.GetType();
            InstanceTypeInfo = InstanceType.GetTypeInfo();

            NotifyPropertyChanged = instance as INotifyPropertyChanged;
        }

        private void HookIntoChangeHandlers()
        {
            if (NotifyPropertyChanged != null)
                NotifyPropertyChanged.PropertyChanged += (s, e) => OnPropertyChanged(s, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        public object GetPropertyValue(string propertyPath)
        {
            if (!ReflectionHelpers.TryGetMemberPathValue(Instance, InstanceTypeInfo, propertyPath, out var result, false, true, path => GetPropertyValue(path)))
                return null;
            return result;
        }

        public void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged?.RaiseEvent("PropertyChanged", e);
        }
    }
}
