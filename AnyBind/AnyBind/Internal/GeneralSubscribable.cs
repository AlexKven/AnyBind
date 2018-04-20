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

        public List<IInstanceAdapter> Adapters { get; } = new List<IInstanceAdapter>();

        internal GeneralSubscribable(object instance, DependencyManager dependencyManager)
        {
            Instance = instance;
            InstanceType = instance.GetType();
            InstanceTypeInfo = InstanceType.GetTypeInfo();
            HookIntoChangeHandlers();
        }

        private void HookIntoChangeHandlers()
        {
            bool subscribed = false;
            for (int i = 0; i < ClassAdapters.Count && !subscribed; i++)
            {
                var adapter = ClassAdapters[i];
                if (adapter.CanSubscribe(InstanceTypeInfo))
                {
                    var instanceAdapter = adapter.CreateInstanceAdapter(Instance);
                    instanceAdapter.SubscribeToProperties(InstanceTypeInfo.DeclaredProperties.Select(pi => pi.Name).ToArray());
                    subscribed = true;
                }
            }
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
            PropertyChanged?.RaiseEvent("PropertyChanged", e);
        }

        public static List<IClassAdapter> ClassAdapters = new List<IClassAdapter>() { new NotifyPropertyChangedClassAdapter() };

        public static bool CanSubscribe(TypeInfo typeInfo)
        {
            return ClassAdapters.Any(a => a.CanSubscribe(typeInfo));
        }

        public static IEnumerable<string> FilterSubscribableProperties(TypeInfo typeInfo, IEnumerable<string> properties)
        {
            IEnumerable<string> result = new string[0];
            foreach (var adapter in ClassAdapters)
            {
                result = result.Union(adapter.FilterSubscribableProperties(typeInfo, properties));
            }
            return result;
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
