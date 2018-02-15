﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using AnyBind.Internal;
using AnyBind.Attributes;
using System.ComponentModel;

[assembly: InternalsVisibleTo("AnyBind.Tests")]
namespace AnyBind
{
    public static class BindingManager
    {
        private static ConcurrentDictionary<Type, Dictionary<string, List<DependencyBase>>> Registrations
            = new ConcurrentDictionary<Type, Dictionary<string, List<DependencyBase>>>();

        private static ConcurrentDictionary<long, WeakReference> References
            = new ConcurrentDictionary<long, WeakReference>();

        private static ConcurrentDictionary<long, WeakEventSubscriber> Subscribers
            = new ConcurrentDictionary<long, WeakEventSubscriber>();

        private static long NextReferenceId = 0;

        internal struct TypeProperty
        {
            Type Type { get; }
            string Property { get; }
        }

        public static void RegisterClass(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            
            var propertyRegistrations = new Dictionary<string, List<DependencyBase>>();
            foreach (var property in typeInfo.DeclaredProperties)
            {
                SortedSet<string> dependsOn = new SortedSet<string>();

                void AddDependency(string path)
                {
                    while (path != "")
                    {
                        dependsOn.Add(path);
                        if (path.Contains("."))
                            path = path.Substring(0, path.LastIndexOf("."));
                        else
                            path = "";
                    }
                }

                foreach (var att in property.GetCustomAttributes<DependsOnAttribute>())
                {
                    foreach (var prop in att.PropertyPaths)
                        AddDependency(prop);
                }

                foreach (var path in dependsOn)
                {
                    if (propertyRegistrations.TryGetValue(path, out var dependencies))
                        dependencies.Add(new PropertyDependency(property.Name));
                    else
                        propertyRegistrations[path] = new List<DependencyBase>() { new PropertyDependency(property.Name) };
                }
            }
            Registrations.TryAdd(type, propertyRegistrations);
        }

        public static void SetupBindings<T>(T instance)
        {
            var registrations = Registrations[typeof(T)];
            var typeInfo = instance?.GetType().GetTypeInfo();

            var id = NextReferenceId++;
            References.TryAdd(id, new WeakReference(instance));

            var subscriber = new WeakEventSubscriber(instance, (s, p, fp) =>
            {
                OnUpdate(s, p[0].ToString(), (long)fp);
            });
            Subscribers.TryAdd(id, subscriber);

            foreach (var registration in registrations)
            {
                var parent = GetParentOfSubentity(instance, typeInfo, registration.Key);
                var parentTypeInfo = parent?.GetType().GetTypeInfo();
                if (parentTypeInfo.ImplementedInterfaces.Contains(typeof(INotifyPropertyChanged)))
                {
                    subscriber.Subscribe(ReflectionHelpers.SearchTypeAndBase(parentTypeInfo,
                        ti => ti.GetDeclaredEvent("PropertyChanged")), parent, registration.Value.Select(db => ((PropertyDependency)db).PropertyName));
                }
            }
        }

        internal static void RegisterChangeHandler(object instance, TypeInfo typeInfo, string propertyName)
        {
            
        }

        internal static void OnUpdate(object target, string propertyName, long referenceId)
        {
            //foreach (var property in propertiesToUpdate)
            //{
            //    target.RaiseEvent<PropertyChangedEventArgs>("PropertyChanged", new PropertyChangedEventArgs(property));
            //}
        }

        internal static object GetParentOfSubentity(object instance, TypeInfo typeInfo, string path)
        {
            var splitPath = path.Split('.').ToList();
            while (instance != null && splitPath.Count > 1)
            {
                var member = splitPath[0];
                splitPath.RemoveAt(0);
                ReflectionHelpers.TryGetMemberValue(instance, typeInfo, member, out var next);
                instance = next;
                typeInfo = next.GetType().GetTypeInfo();
            }
            return instance;
        }
    }
}
