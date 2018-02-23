using AnyBind.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AnyBind
{
    internal class SubscribableHandler
    {
        private WeakReference<ISubscribable> Instance;
        private Type InstanceType;
        private TypeInfo InstanceTypeInfo;
        private Dictionary<string, PropertyChangedEventHandler> ChangeHandlerDelegates = new Dictionary<string, PropertyChangedEventHandler>();
        private Dictionary<string, List<string>> PropertyDependencies = new Dictionary<string, List<string>>();
        private Dictionary<string, WeakReference<ISubscribable>> SubscribablePropertyCache = new Dictionary<string, WeakReference<ISubscribable>>();

        public SubscribableHandler(ISubscribable instance)
        {
            Instance = new WeakReference<ISubscribable>(instance);
            InstanceType = instance.GetType();
            InstanceTypeInfo = InstanceType.GetTypeInfo();
            instance.PropertyChanged += GetChangeHandlerDelegate(this, "");
            
            foreach (var dependency in DependencyManager.Registrations[InstanceType])
            {
                switch (dependency.Key)
                {
                    case PropertyDependency propertyDependency:
                        PropertyDependencies.Add(propertyDependency.PropertyName, dependency.Value);
                        break;
                }
            }
        }

        bool TryGetSubscribablePropertyCache(string propertyPath, out ISubscribable result)
        {
            if (SubscribablePropertyCache.TryGetValue(propertyPath, out var weak))
            {
                if (weak.TryGetTarget(out result))
                    return true;
            }
            result = null;
            return false;
        }

        bool TryGetSubscribablePropertyCache(string propertyPath, object value)
        {
            if (value is ISubscribable typedValue)
            {
                if (SubscribablePropertyCache.ContainsKey(propertyPath))
                    SubscribablePropertyCache[propertyPath] = new WeakReference<ISubscribable>(typedValue);
                else
                    SubscribablePropertyCache.Add(propertyPath, new WeakReference<ISubscribable>(typedValue));
                return true;
            }
            return false;
        }

        private static PropertyChangedEventHandler GetChangeHandlerDelegate(SubscribableHandler instance, string senderPath)
        {
            PropertyChangedEventHandler result;
            if (instance.ChangeHandlerDelegates.TryGetValue(senderPath, out result))
                return result;
            else
            {
                var weakInstance = new WeakReference<SubscribableHandler>(instance);
                result = (s, e) =>
                {
                    if (weakInstance.TryGetTarget(out var target))
                    {
                        target.OnPropertyChanged(senderPath, e);
                    }
                };
            }
            instance.ChangeHandlerDelegates.Add(senderPath, result);
            return result;
        }

        private void RaisePropertyChanged(string propertyPath, bool secondaryEvent)
        {
            ISubscribable instance;
            if (Instance.TryGetTarget(out instance))
            {
                string objectPath = "";
                string propertyName = propertyPath;
                if (propertyPath.Contains("."))
                {
                    var lastIndex = propertyPath.LastIndexOf('.');
                    objectPath = propertyPath.Substring(0, lastIndex);
                    propertyName = propertyPath.Substring(lastIndex + 1);
                }

                if (objectPath != "")
                    return;

                var e = new DependentPropertyChangedEventArgs(propertyName, objectPath, secondaryEvent);
                instance.RaisePropertyChanged(e);
            }
        }

        private void CheckSubpropertyChangeHandlers(string propertyPath)
        {
            ISubscribable instance;
            if (Instance.TryGetTarget(out instance))
            {
                if (instance.SubscribableProperties.Contains(propertyPath)
                    && TryGetSubscribablePropertyCache(propertyPath, out var cached)
                    && cached != null
                    && ChangeHandlerDelegates.ContainsKey(propertyPath))
                {
                    cached.PropertyChanged -= ChangeHandlerDelegates[propertyPath];
                }

                if (instance.SubscribableProperties.Contains(propertyPath))
                {
                    var propertyValue = instance.GetPropertyValue(propertyPath);
                    if (propertyValue is ISubscribable typedPropertyValue
                        && TryGetSubscribablePropertyCache(propertyPath, typedPropertyValue))
                    {
                        typedPropertyValue.PropertyChanged += GetChangeHandlerDelegate(this, propertyPath);
                    }
                }
            }
        }

        private void OnPropertyChanged(string path, PropertyChangedEventArgs e)
        {
            string propertyPath = $"{path}.{e.PropertyName}".Trim('.');
            bool updateProperties = true;

            CheckSubpropertyChangeHandlers(propertyPath);

            if (e is DependentPropertyChangedEventArgs)
            {
                var typedE = (DependentPropertyChangedEventArgs)e;
                if (typedE.ObjectPath == path && typedE.SecondaryEvent)
                    updateProperties = false;
            }

            if (updateProperties)
            {
                foreach (var dependent in GetFullListOfDependents(propertyPath).Distinct())
                {
                    RaisePropertyChanged(dependent, true);
                }
            }
        }

        private IEnumerable<string> GetFullListOfDependents(string propertyPath)
        {
            var subKeys = PropertyDependencies.Keys.Where(key => key.StartsWith($"{propertyPath}."));
            if (PropertyDependencies.TryGetValue(propertyPath, out var dependents))
            {
                foreach (var dependent in dependents)
                    yield return dependent;
            }
            foreach (var subKey in subKeys)
            {
                foreach (var dependent in PropertyDependencies[subKey])
                    yield return dependent;
            }
        }
    }
}
