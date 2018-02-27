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
                        var name = propertyDependency.PropertyName;
                        PropertyDependencies.Add(name, dependency.Value);
                        break;
                }
            }

            CachePropertyPath("", instance);
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

        bool TryAddToSubscribablePropertyCache(string propertyPath, object value)
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

        private void BreakIntoPropertyNameAndPath(string propertyPath, out string propertyName, out string objectPath)
        {
            if (propertyPath.EndsWith("]"))
            {
                var indexOpen = propertyPath.LastIndexOf("[");
                objectPath = propertyPath.Substring(0, indexOpen);
                propertyName = propertyPath.Substring(indexOpen + 1);
            }
            else if (propertyPath.Contains("."))
            {
                var lastIndex = propertyPath.LastIndexOf('.');
                objectPath = propertyPath.Substring(0, lastIndex);
                propertyName = propertyPath.Substring(lastIndex + 1);
            }
            objectPath = "";
            propertyName = propertyPath;
        }

        private void RaisePropertyChanged(string propertyPath, bool secondaryEvent)
        {
            ISubscribable instance;
            if (Instance.TryGetTarget(out instance))
            {
                BreakIntoPropertyNameAndPath(propertyPath, out var propertyName, out var objectPath);

                if (objectPath != "")
                    return;

                var e = new DependentPropertyChangedEventArgs(propertyName, objectPath, secondaryEvent);
                instance.RaisePropertyChanged(e);
            }
        }

        private void UnCachePropertyPath(string propertyPath)
        {
            foreach (var path in SubscribablePropertyCache.Keys.Where(key => key.StartsWith(propertyPath)))
            {
                // if nocache
                if (TryGetSubscribablePropertyCache(path, out var cached)
                    && cached != null
                    && ChangeHandlerDelegates.ContainsKey(path))
                {
                    cached.PropertyChanged -= ChangeHandlerDelegates[propertyPath];
                }
            }
        }

        private void CachePropertyPath(string propertyPath, ISubscribable instance)
        {
            foreach (var path in PropertyDependencies.Keys.Where(key => key.StartsWith(propertyPath)))
            {
                // if nocache
                if (true)
                {
                    var propertyValue = instance.GetPropertyValue(path);
                    if (propertyValue is ISubscribable typedPropertyValue
                        && TryAddToSubscribablePropertyCache(path, typedPropertyValue))
                    {
                        typedPropertyValue.PropertyChanged += GetChangeHandlerDelegate(this, path);
                    }
                }
            }
        }

        private void CheckSubpropertyChangeHandlers(string propertyPath)
        {
            UnCachePropertyPath(propertyPath);
            ISubscribable instance;
            if (Instance.TryGetTarget(out instance))
            {
                CachePropertyPath(propertyPath, instance);
            }
        }

        private void OnPropertyChanged(string path, PropertyChangedEventArgs e)
        {
            string propertyPath;
            if (e.PropertyName.StartsWith("[") || path == "")
                propertyPath = $"{path}{e.PropertyName}".Trim('.');
            else
                propertyPath = $"{path}.{e.PropertyName}".Trim('.');
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
            List<string> search = new List<string>() { propertyPath };
            search.AddRange(PropertyDependencies.Keys.Where(key => key.StartsWith($"{propertyPath}.")));
            var result = PropertyDependencies.FindDependencyBranches(search.ToArray());
            result.Remove(propertyPath);
            return result;
        }
    }
}
