using AnyBind.Caching;
using AnyBind.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AnyBind
{
    internal class SubscribableHandler : IDisposable
    {
        private DependencyManager Manager;
        private WeakReference<ISubscribable> Instance;
        private Type InstanceType;
        private TypeInfo InstanceTypeInfo;
        private Dictionary<string, PropertyChangedEventHandler> ChangeHandlerDelegates = new Dictionary<string, PropertyChangedEventHandler>();
        private Dictionary<string, List<string>> PropertyDependencies = new Dictionary<string, List<string>>();

        private WeakReferenceCache<string, ISubscribable> SubscribablePropertyCache = new WeakReferenceCache<string, ISubscribable>();
        private StrongReferenceCache<string, List<string>> SubscribedConstantIndexCache = new StrongReferenceCache<string, List<string>>();
        private StrongReferenceCache<string, StrongReferenceCache<string, string>> SubscribedVariableIndexCache = new StrongReferenceCache<string, StrongReferenceCache<string, string>>();

        //private IEnumerable<string> GetAllSubscribedIndices(string path)
        //{
        //    if (SubscribedConstantIndexCache.TryGetValue(path, out var list1))
        //    {
        //        foreach (var item in list1)
        //            yield return item;
        //    }
        //    if (SubscribedVariableIndexCache.TryGetValue(path, out var list2))
        //    {
        //        foreach (var item in list2)
        //        {
        //            yield return item.Item2;
        //        }
        //    }
        //}

        public SubscribableHandler(DependencyManager manager, ISubscribable instance)
        {
            Manager = manager;
            manager.StronglyReference(this);
            Instance = new WeakReference<ISubscribable>(instance);
            InstanceType = instance.GetType();
            InstanceTypeInfo = InstanceType.GetTypeInfo();

            SubscribablePropertyCache.SetActions.Add(SubscribablePropertyCache_InitializeBindingInstance);
            SubscribablePropertyCache.SetActions.Add(SubscribablePropertyCache_SubscribeToEvent);
            SubscribablePropertyCache.SetActions.Add(SubscribablePropertyCache_InitializeBindingInstance);
            SubscribablePropertyCache.RemoveActions.Add(SubscribablePropertyCache_UnsubscribeFromEvent);

            foreach (var dependency in manager.GetRegistrations(instance.GetSubscribableType()))
            {
                switch (dependency.Key)
                {
                    case PropertyDependency propertyDependency:
                        var name = propertyDependency.PropertyName;
                        foreach (var str in dependency.Value)
                        {
                            PropertyDependencies.SafeAddToDictionaryOfList(str, name);
                            if (name.GetIndexAndParent(out var parent, out var index))
                            {
                                if (index.StartsWith("<"))
                                {
                                    var indexPath = index.Substring(1, index.Length - 2);
                                    var indexValue = instance.GetPropertyValue(indexPath)?.ToString();
                                    if (!SubscribedVariableIndexCache.ContainsKey(parent))
                                    {
                                        SubscribedVariableIndexCache[parent] = new StrongReferenceCache<string, string>();
                                    }
                                    SubscribedVariableIndexCache[parent][index] = indexValue;
                                }
                                else
                                {
                                    SubscribedConstantIndexCache.SafeAddToCacheOfList(parent, index);
                                }
                            }
                        }
                        break;
                }
            }

            CachePropertyPath("", instance);
        }

        private void SubscribablePropertyCache_InitializeBindingInstance(string path, ISubscribable value)
        {
            if (path != "")
                Manager.InitializeInstance(value);
        }

        private void SubscribablePropertyCache_SubscribeToEvent(string path, ISubscribable value)
        {
            value.PropertyChanged += GetChangeHandlerDelegate(this, path);
        }

        private void SubscribablePropertyCache_UnsubscribeFromEvent(string path, ISubscribable value)
        {
            if (ChangeHandlerDelegates.ContainsKey(path))
            {
                value.PropertyChanged -= ChangeHandlerDelegates[path];
            }
        }

        public void Dispose()
        {
            if (Instance.TryGetTarget(out var instance))
                UnCachePropertyPath("", instance);
            else
                UnCachePropertyPath("");
            ChangeHandlerDelegates.Clear();
            Manager.ReleaseStrongReference(this);
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

        private void UnCachePropertyPath(string propertyPath, ISubscribable instance = null)
        {
            foreach (var path in SubscribablePropertyCache.Keys.Where(key => key.StartsWith(propertyPath)).ToArray())
            {
                SubscribablePropertyCache.TryClearValue(path);
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

                    var subscribableProperties = Manager.FilterSubscribableProperties(typeInfo, nextProperties);
                    
                    foreach (var prop in subscribableProperties)
                        pathsToSubscribe.Push(path.Concat(new string[] { prop }));
                    if (Manager.CanSubscribe(typeInfo))
                    {
                        var typedPropertyValue = GeneralSubscribable.CreateSubscribable(propertyValue, Manager);

                        //if (SubscribablePropertyCache.TryGetValue(reassembled, out var old))
                        //{
                        //    foreach (var index in GetAllSubscribedIndices(reassembled))
                        //    {
                        //        old.SubscribeToIndexedProperty(reassembled, index);
                        //    }
                        //}

                        SubscribablePropertyCache.TrySetValue(reassembled, typedPropertyValue);

                        //foreach (var index in GetAllSubscribedIndices(reassembled))
                        //{
                        //    typedPropertyValue.SubscribeToIndexedProperty(reassembled, index);
                        //}
                    }
                }
            }
        }

        private void CheckSubpropertyChangeHandlers(string propertyPath)
        {
            if (Instance.TryGetTarget(out var instance))
            {
                UnCachePropertyPath(propertyPath, instance);
                CachePropertyPath(propertyPath, instance);
            }
            else
                UnCachePropertyPath(propertyPath);
            
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
