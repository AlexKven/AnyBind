using AnyBind.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AnyBind.Adapters
{
    class ObservableCollectionClassAdapter : IClassAdapter
    {
        public bool CanSubscribe(TypeInfo typeInfo)
        {
            return GetObservableCollectionType(typeInfo) != null;
        }

        public IInstanceAdapter CreateInstanceAdapter(object instance)
        {
            var type = typeof(ObservableCollectionInstanceAdapter<>);
            type = type.MakeGenericType(GetObservableCollectionType(instance.GetType().GetTypeInfo()));
            return (IInstanceAdapter)type.GetTypeInfo().DeclaredConstructors.First().
                Invoke(new object[] { instance });
        }

        public IEnumerable<string> FilterSubscribableProperties(TypeInfo typeInfo, IEnumerable<string> properties)
        {
            var observableType = GetObservableCollectionType(typeInfo);
            if (observableType == null)
                yield break;
            foreach (var property in properties)
            {
                if (property == "Count" || property == "[]")
                    yield return property;
                else if (property.StartsWith("[") && property.EndsWith("]"))
                {
                    string middle = property.Substring(1, property.Length - 2);
                    if (typeof(IConvertible).GetTypeInfo().IsAssignableFrom(observableType.GetTypeInfo()))
                    {
                        bool canConvert = false;
                        try
                        {
                            Convert.ChangeType(middle, observableType);
                            canConvert = true;
                        }
                        catch (Exception) { }
                        if (canConvert)
                            yield return property;
                    }
                }
            }
        }

        private Type GetObservableCollectionType(TypeInfo typeInfo)
        {
            return ReflectionHelpers.SearchTypeAndBase(typeInfo, ti =>
            {
                if (ti.IsGenericType && ti.GenericTypeArguments.Length == 1 && ti.GetGenericTypeDefinition() == typeof(ObservableCollection<>))
                    return ti.GenericTypeArguments[0];
                return null;
            });
        }
    }
}
