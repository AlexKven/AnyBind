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
    public class DependencyManager
    {
        private ConcurrentDictionary<Type, Dictionary<DependencyBase, List<string>>> Registrations
            = new ConcurrentDictionary<Type, Dictionary<DependencyBase, List<string>>>();

        private List<SubscribableHandler> SetupInstances = new List<SubscribableHandler>();

        public virtual Dictionary<DependencyBase, List<string>> GetRegistrations(Type type)
        {
            if (!Registrations.TryGetValue(type, out var result))
                throw new KeyNotFoundException($"No such class as {type} was registered.");
            return result;
        }

        public void RegisterClass(Type type)
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

        public static void SetupBindings(object instance)
        {
            
        }
    }
}
