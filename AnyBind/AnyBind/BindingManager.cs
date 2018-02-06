using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AnyBind
{
    public class BindingManager
    {
        internal abstract class DependencyBase
        {
            public abstract bool TryHookHandler(object applyTo);
        }

        internal class PropertyDependency : DependencyBase
        {
            public string PropertyName { get; }

            public PropertyDependency(string propertyName)
            {
                PropertyName = propertyName;
            }

            public override bool TryHookHandler(object applyTo)
            {
                throw new NotImplementedException();
            }
        }

        private static ConcurrentDictionary<Type, Dictionary<string, List<DependencyBase>>> Registrations;

        public struct TypeProperty
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
                    var dependencies = propertyRegistrations[path];
                    if (dependencies == null)
                        propertyRegistrations[path] = new List<DependencyBase>() { new PropertyDependency(property.Name) };
                    dependencies.Add(new PropertyDependency(property.Name));
                }
            }
            Registrations.TryAdd(type, propertyRegistrations);
        }

        public static void SetupBindings<T>(T instance)
        {
            var registrations = Registrations[typeof(T)];

            foreach (var registration in registrations)
            {

            }
        }

        internal static object GetParent(object instance, TypeInfo typeInfo, string path)
        {
            var splitPath = path.Split('.').ToList();
            while (instance != null && splitPath.Count > 1)
            {
                var member = splitPath[0];
                splitPath.RemoveAt(0);
                //TryGetMemberValue(instance, type, member, out var next);
                //instance = next;
                //type = next.GetType();

            }
            return instance;
        }
    }
}
