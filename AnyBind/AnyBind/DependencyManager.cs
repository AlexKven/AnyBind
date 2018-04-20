﻿using AnyBind.Adapters;
using AnyBind.Attributes;
using AnyBind.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

[assembly:InternalsVisibleTo("AnyBind.Tests")]
namespace AnyBind
{
    public class DependencyManager
    {
        protected ConcurrentDictionary<Type, Dictionary<DependencyBase, List<string>>> Registrations
            = new ConcurrentDictionary<Type, Dictionary<DependencyBase, List<string>>>();

        protected Dictionary<Type, Dictionary<DependencyBase, Dictionary<string, Type>>> PreRegistrations
            = new Dictionary<Type, Dictionary<DependencyBase, Dictionary<string, Type>>>();

        private ConditionalWeakTable<object, SubscribableHandler> SetupInstances = new ConditionalWeakTable<object, SubscribableHandler>();

        private List<SubscribableHandler> StrongSubscribableHandlerReferences = new List<SubscribableHandler>();

        private Dictionary<IClassAdapter, int> PreRegisteredClassAdapters = new Dictionary<IClassAdapter, int>();

        private List<IClassAdapter> RegisteredClassAdapters = new List<IClassAdapter>();

        public IReadOnlyCollection<IClassAdapter> GetClassAdapters() =>
            new System.Collections.ObjectModel.ReadOnlyCollection<IClassAdapter>(RegisteredClassAdapters);

        public DependencyManager()
        {
            PreRegisteredClassAdapters.Add(new Adapters.NotifyPropertyChangedClassAdapter(), 0);
        }

        public virtual Dictionary<DependencyBase, List<string>> GetRegistrations(Type type)
        {
            if (!Registrations.TryGetValue(type, out var result))
                throw new KeyNotFoundException($"No such class as {type} was registered.");
            return result;
            
        }

        public virtual void InitializeInstance(object instance)
        {
            if (!SetupInstances.TryGetValue(instance, out _))
            {
                var subscribable = GeneralSubscribable.CreateSubscribable(instance, this);
                SetupInstances.Add(instance, null);
                var handler = new SubscribableHandler(this, subscribable);
                SetupInstances.Remove(instance);
                SetupInstances.Add(instance, handler);
            }
        }

        public virtual void DeactivateInstance(object instance)
        {
            if (SetupInstances.TryGetValue(instance, out var result))
            {
                SetupInstances.Remove(instance);
                result.Dispose();
            }
        }

        internal void StronglyReference(SubscribableHandler handler)
        {
            StrongSubscribableHandlerReferences.Add(handler);
        }

        internal void ReleaseStrongReference(SubscribableHandler handler)
        {
            StrongSubscribableHandlerReferences.Remove(handler);
        }

        /// <summary>
        /// Registers an IClassAdapter which allows you to subscribe to property changes on
        /// any class that has any sort of mechanism for listening to property changes.
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="priority"></param>
        public void RegisterClassAdapter(IClassAdapter adapter, int priority)
        {
            PreRegisteredClassAdapters.Add(adapter, priority);
        }

        public void RegisterClass(Type type)
        {
            var typeInfo = type.GetTypeInfo();

            var propertyPreRegistrations = new Dictionary<DependencyBase, Dictionary<string, Type>>();
            foreach (var property in typeInfo.DeclaredProperties)
            {
                Dictionary<string, Type> dependsOn = new Dictionary<string, Type>();

                foreach (var att in property.GetCustomAttributes<DependsOnAttribute>())
                {
                    foreach (var prop in att.PropertyPaths)
                        dependsOn.Add(prop, ReflectionHelpers.GetTypeOfPath(type, prop.DisassemblePropertyPath()));
                }

                foreach (var pathNType in dependsOn)
                {
                    var dependency = new PropertyDependency(property.Name);
                    if (propertyPreRegistrations.TryGetValue(dependency, out var dependencies))
                        dependencies.Add(pathNType.Key, pathNType.Value);
                    else
                        propertyPreRegistrations[dependency] = new Dictionary<string, Type>() { { pathNType.Key, pathNType.Value } };
                }
            }
            PreRegistrations.Add(type, propertyPreRegistrations);
        }

        public void FinalizeRegistrations()
        {
            foreach (var typeRegistration in PreRegistrations)
            {
                Dictionary<string, List<string>> dependencies = new Dictionary<string, List<string>>();
                void addDependency(string propertyPath, string dependent)
                {
                    if (dependencies.TryGetValue(propertyPath, out var list))
                    {
                        if (!list.Contains(dependent))
                            list.Add(dependent);
                    }
                    else
                        dependencies.Add(propertyPath, new List<string>() { dependent });
                }

                foreach (var dependency in typeRegistration.Value)
                {
                    switch (dependency.Key)
                    {
                        case PropertyDependency pd:
                            foreach (var dependentOn in dependency.Value)
                            {
                                addDependency(pd.PropertyName, dependentOn.Key);
                                var chainL = dependentOn.Key.DisassemblePropertyPath();
                                var chainR = chainL;
                                var length = chainL.Count();
                                while (length > 1)
                                {
                                    chainR = chainR.Take(length - 1);
                                    addDependency(chainL.ReassemblePropertyPath(), chainR.ReassemblePropertyPath());
                                    chainL = chainL.Take(length - 1);
                                    length--;
                                }
                            }
                            break;
                    }
                }

                var registration = new Dictionary<DependencyBase, List<string>>();
                foreach (var dependency in dependencies)
                {
                    registration.Add(new PropertyDependency(dependency.Key), dependency.Value);
                }

                Registrations.TryAdd(typeRegistration.Key, registration);

                RegisteredClassAdapters.AddRange(PreRegisteredClassAdapters.OrderBy(kvp => kvp.Value).Select(kvp => kvp.Key));
            }
        }

        internal bool CanSubscribe(TypeInfo typeInfo)
        {
            return RegisteredClassAdapters.Any(a => a.CanSubscribe(typeInfo));
        }

        internal IEnumerable<string> FilterSubscribableProperties(TypeInfo typeInfo, IEnumerable<string> properties)
        {
            IEnumerable<string> result = new string[0];
            foreach (var adapter in RegisteredClassAdapters)
            {
                result = result.Union(adapter.FilterSubscribableProperties(typeInfo, properties));
            }
            return result;
        }
    }
}
