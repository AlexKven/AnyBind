using System;
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
        private static ConcurrentDictionary<Type, Dictionary<DependencyBase, List<string>>> Registrations
            = new ConcurrentDictionary<Type, Dictionary<DependencyBase, List<string>>>();

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
    }
}
