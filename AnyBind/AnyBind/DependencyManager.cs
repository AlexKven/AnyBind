using AnyBind.Attributes;
using AnyBind.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AnyBind
{
    public static class DependencyManager
    {
        internal static ConcurrentDictionary<Type, Dictionary<DependencyBase, List<string>>> Registrations
            = new ConcurrentDictionary<Type, Dictionary<DependencyBase, List<string>>>();

        private static ConcurrentDictionary<long, WeakReference> References
            = new ConcurrentDictionary<long, WeakReference>();

        private static long NextReferenceId = 0;

        internal struct TypeProperty
        {
            Type Type { get; }
            string Property { get; }
        }

        public static void RegisterClass(Type type)
        {
            var typeInfo = type.GetTypeInfo();

            var propertyRegistrations = new Dictionary<DependencyBase, List<string>>();
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
                    var dependency = new PropertyDependency(property.Name);
                    if (propertyRegistrations.TryGetValue(dependency, out var dependencies))
                        dependencies.Add(property.Name);
                    else
                        propertyRegistrations[dependency] = new List<string>() { property.Name };
                }
            }
            Registrations.TryAdd(type, propertyRegistrations);
        }

        public static void SetupBindings<T>(T instance) where T : ISubscribable
        {
            var registrations = Registrations[typeof(T)];
            var typeInfo = instance?.GetType().GetTypeInfo();

            var id = NextReferenceId++;
            References.TryAdd(id, new WeakReference(instance));

            var subscriber = new WeakEventSubscriber(instance, (s, p, fp) =>
            {
                OnUpdate(s, p[0].ToString(), (IEnumerable<string>)fp);
            });
            Subscribers.TryAdd(id, subscriber);

            foreach (var registration in registrations)
            {
                switch (registration.Key)
                {
                    case PropertyDependency propertyDependency:
                        var parent = GetParentOfSubentity(instance, typeInfo, propertyDependency.PropertyName);
                        var parentTypeInfo = parent?.GetType().GetTypeInfo();
                        if (parentTypeInfo.ImplementedInterfaces.Contains(typeof(INotifyPropertyChanged)))
                        {
                            subscriber.Subscribe(ReflectionHelpers.SearchTypeAndBase(parentTypeInfo,
                                ti => ti.GetDeclaredEvent("PropertyChanged")), parent, registration.Value);
                        }
                        break;
                }
            }
        }

        internal static void RegisterChangeHandler(object instance, TypeInfo typeInfo, string propertyName)
        {

        }

        internal static void OnUpdate(object target, string sendingProperty, IEnumerable<string> propertiesToUpdate)
        {
            foreach (var property in propertiesToUpdate)
            {
                target.RaiseEvent<PropertyChangedEventArgs>("PropertyChanged", new PropertyChangedEventArgs(property));
            }
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
