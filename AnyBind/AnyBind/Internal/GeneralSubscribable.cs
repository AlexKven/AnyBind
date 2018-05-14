using AnyBind.Adapters;
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

        public List<(IInstanceAdapter, string[])> Adapters { get; } = new List<(IInstanceAdapter, string[])>();

        internal GeneralSubscribable(object instance, DependencyManager dependencyManager)
        {
            Instance = instance;
            InstanceType = instance.GetType();
            InstanceTypeInfo = InstanceType.GetTypeInfo();
            HookIntoChangeHandlers(dependencyManager);
        }

        private void HookIntoChangeHandlers(DependencyManager dependencyManager)
        {
            var properties = InstanceType.GetRuntimeProperties()
                .Select(pi => (pi.Name == "Item" && pi.GetIndexParameters().Length > 0) ? "[]" : pi.Name).ToList();
            IEnumerable<string> toBeSubscribed = properties;
            for (int i = 0; i < dependencyManager.GetClassAdapters().Count; i++)
            {
                var adapter = dependencyManager.GetClassAdapters().ElementAt(i);
                if (adapter.CanSubscribe(InstanceTypeInfo))
                {
                    var subscribableProperties = adapter.FilterSubscribableProperties(InstanceTypeInfo, properties).ToArray();
                    var instanceAdapter = adapter.CreateInstanceAdapter(Instance);
                    instanceAdapter.PropertyChanged += (s, e) => OnPropertyChanged(s, e);
                    Adapters.Add((instanceAdapter, subscribableProperties));
                    var subscribed = instanceAdapter.SubscribeToProperties(subscribableProperties.Intersect(toBeSubscribed).ToArray());
                    toBeSubscribed = toBeSubscribed.Except(subscribed);
                }
            }
            toBeSubscribed.ToArray();
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
            Instance?.RaiseEvent("PropertyChanged", e);
        }

        public static ISubscribable CreateSubscribable(object obj, DependencyManager dependencyManager)
        {
            if (obj is ISubscribable subscribable)
                return subscribable;
            return new GeneralSubscribable(obj, dependencyManager);
        }

        public Type GetSubscribableType() => InstanceType;
    }
}
