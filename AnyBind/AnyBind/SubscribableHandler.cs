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
        private DependencyManager Manager;
        private WeakReference<ISubscribable> Instance;
        private Type InstanceType;
        private TypeInfo InstanceTypeInfo;
        private Dictionary<string, PropertyChangedEventHandler> ChangeHandlerDelegates = new Dictionary<string, PropertyChangedEventHandler>();
        private Dictionary<string, List<string>> PropertyDependencies = new Dictionary<string, List<string>>();
        private Dictionary<string, WeakReference<ISubscribable>> SubscribablePropertyCache = new Dictionary<string, WeakReference<ISubscribable>>();

        public SubscribableHandler(DependencyManager manager, ISubscribable instance)
        {
            Manager = manager;
            Instance = new WeakReference<ISubscribable>(instance);
            InstanceType = instance.GetType();
            InstanceTypeInfo = InstanceType.GetTypeInfo();

            foreach (var dependency in manager.GetRegistrations(instance.GetSubscribableType()))
            {
                switch (dependency.Key)
                {
                    case PropertyDependency propertyDependency:
                        var name = propertyDependency.PropertyName;
                        foreach (var str in dependency.Value)
                            PropertyDependencies.SafeAddToDictionaryOfList(str, name);
                        break;
                }
            }

            CachePropertyPath("", instance);
        }

        public void Unsubscribe()
        {
            UnCachePropertyPath("");
            ChangeHandlerDelegates.Clear();
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

        private void RaisePropertyChanged(string propertyPath, bool secondaryEvent)
        {
            ISubscribable instance;
            if (Instance.TryGetTarget(out instance))
            {
                var objectPath = propertyPath.DisassemblePropertyPath().GetParentOfPropertyPath(out var propertyName);

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
                    cached.PropertyChanged -= ChangeHandlerDelegates[path];
                }
            }
        }

        private void CachePropertyPath(string propertyPath, ISubscribable instance)
        {
            Stack<IEnumerable<string>> pathsToSubscribe = new Stack<IEnumerable<string>>();
            pathsToSubscribe.Push(propertyPath.DisassemblePropertyPath());
            var possiblePaths = PropertyDependencies.Keys.Where(key => key.StartsWith(propertyPath));
            IEnumerable<string> path;
            while (pathsToSubscribe.Count > 0)
            {
                path = pathsToSubscribe.Pop();
                // if nocache
                if (true)
                {
                    var compLength = path.Count();
                    var reassembled = path.ReassemblePropertyPath();
                    var nextProperties = possiblePaths.Where(pth => pth.StartsWith(reassembled))
                        .Select(pth => pth.DisassemblePropertyPath().Skip(compLength).FirstOrDefault())
                        .Where(pth => pth != null).Distinct();

                    object propertyValue;
                    if (reassembled == "")
                        propertyValue = instance;
                    else
                        propertyValue = instance.GetPropertyValue(reassembled);
                    var typeInfo = propertyValue?.GetType()?.GetTypeInfo();

                    var subscribableProperties = GeneralSubscribable.FilterSubscribableProperties(typeInfo, nextProperties);
                    
                    foreach (var prop in subscribableProperties)
                        pathsToSubscribe.Push(path.Concat(new string[] { prop }));
                    if (GeneralSubscribable.CanSubscribe(typeInfo))
                    {
                        var typedPropertyValue = GeneralSubscribable.CreateSubscribable(propertyValue);
                        if (TryAddToSubscribablePropertyCache(reassembled, typedPropertyValue))
                        {
                            typedPropertyValue.PropertyChanged += GetChangeHandlerDelegate(this, reassembled);
                            if (reassembled != "")
                                Manager.InitializeInstance(propertyValue);
                        }
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
            var propertyPath = StringHelpers.ReassemblePropertyPath(path, e.PropertyName);

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
                    CheckSubpropertyChangeHandlers(dependent);
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

        public bool IsAlive
        {
            get
            {
                return Instance.TryGetTarget(out _);
            }
        }
    }
}
