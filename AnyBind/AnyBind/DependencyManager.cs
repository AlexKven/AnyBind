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

        private Dictionary<Type, Dictionary<DependencyBase, Dictionary<string, Type>>> PreRegistrations
            = new Dictionary<Type, Dictionary<DependencyBase, Dictionary<string, Type>>>();

        private List<SubscribableHandler> SetupInstances = new List<SubscribableHandler>();

        public virtual Dictionary<DependencyBase, List<string>> GetRegistrations(Type type)
        {
            if (!Registrations.TryGetValue(type, out var result))
                throw new KeyNotFoundException($"No such class as {type} was registered.");
            return result;
        }

        internal virtual Dictionary<DependencyBase, Dictionary<string, Type>> GetPreRegistrations(Type type)
         => PreRegistrations[type];

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

            //public static void SetupBindings(object instance)
            //{

            //}
    }
}
