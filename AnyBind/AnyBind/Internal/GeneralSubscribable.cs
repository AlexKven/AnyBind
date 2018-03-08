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
        public object Instance { get; }
        public Type InstanceType { get; }
        public TypeInfo InstanceTypeInfo { get; }

        private INotifyPropertyChanged NotifyPropertyChanged;

        public GeneralSubscribable(object instance)
        {
            Instance = instance;
            InstanceType = instance.GetType();
            InstanceTypeInfo = InstanceType.GetTypeInfo();

            NotifyPropertyChanged = instance as INotifyPropertyChanged;
            HookIntoChangeHandlers();
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

        public static bool CanSubscribe(TypeInfo typeInfo)
        {
            if (typeof(INotifyPropertyChanged).GetTypeInfo().IsAssignableFrom(typeInfo))
                return true;
            return false;
        }

        public static IEnumerable<string> FilterSubscribableProperties(TypeInfo typeInfo, IEnumerable<string> properties)
        {
            if (typeof(INotifyPropertyChanged).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                foreach (var prop in properties)
                    yield return prop;
            }
        }

        public static ISubscribable CreateSubscribable(object obj)
        {
            if (obj is ISubscribable subscribable)
                return subscribable;
            return new GeneralSubscribable(obj);
        }

        public Type GetSubscribableType() => InstanceType;
    }
}
