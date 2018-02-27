using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AnyBind.Internal
{
    internal static class ReflectionHelpers
    {
        internal static bool TryGetMemberValue(object instance, TypeInfo typeInfo, string memberName, out object memberValue, bool searchFields = true, bool searchProperties = true, Func<string, object> indexerProvider = null)
        {
            if (memberName.StartsWith("[") && memberName.EndsWith("]") && searchProperties)
            {
                var indexer = memberName.Substring(1, memberName.Length - 2);
                if (indexer.StartsWith("<") && indexer.EndsWith(">") && indexerProvider != null)
                {
                    indexer = indexer.Substring(1, indexer.Length - 2);
                    return TryGetIndexedPropertyValue(instance, typeInfo, indexerProvider(indexer), out memberValue);
                }
                return TryGetIndexedPropertyValue(instance, typeInfo, indexer, out memberValue);
            }
            if (searchProperties)
            {
                var property = SearchTypeAndBase(typeInfo, t => t.DeclaredProperties.FirstOrDefault(pi => pi.Name == memberName));
                if (property != null)
                {

                    memberValue = property.GetValue(instance);
                    return true;
                }
            }
            if (searchFields)
            {
                var field = SearchTypeAndBase(typeInfo, t => t.DeclaredFields.FirstOrDefault(fi => fi.Name == memberName));
                if (field != null)
                {
                    memberValue = field.GetValue(instance);
                    return true;
                }
            }
            memberValue = null;
            return false;
        }

        internal static bool TryGetIndexedPropertyValue(object instance, TypeInfo typeInfo, object indexer, out object memberValue)
        {
            var property = SearchTypeAndBase(typeInfo, t => t.DeclaredProperties.FirstOrDefault(pi => pi.Name == "Item"));
            if (property != null)
            {
                ParameterInfo[] parameters;
                if ((parameters = property.GetIndexParameters()).Length == 1)
                {
                    try
                    {
                        memberValue = property.GetValue(instance, new object[] { System.Convert.ChangeType(indexer, parameters[0].ParameterType) });
                        return true;
                    }
                    catch (Exception) { }
                }
            }
            memberValue = null;
            return false;
        }

        internal static T SearchTypeAndBase<T>(TypeInfo typeInfo, Func<TypeInfo, T> searchDelegate) where T : class
        {
            T result = null;
            while (result == null && typeInfo != null)
            {
                result = searchDelegate(typeInfo);
                typeInfo = typeInfo.BaseType?.GetTypeInfo();
            }
            return result;
        }

        internal static void RaiseEvent<TEventArgs>(this object source, string eventName, TEventArgs eventArgs) where TEventArgs : EventArgs
        {
            RaiseEvent(source, source?.GetType().GetTypeInfo(), eventName, eventArgs);
        }

        private static void RaiseEvent<TEventArgs>(object source, TypeInfo typeInfo, string eventName, TEventArgs eventArgs) where TEventArgs : EventArgs
        {
            var eventDelegate = typeInfo.GetDeclaredField(eventName)?.GetValue(source) as MulticastDelegate;

            if (eventDelegate == null)
            {
                typeInfo = typeInfo.BaseType?.GetTypeInfo();
                if (typeInfo != null)
                    RaiseEvent(source, typeInfo, eventName, eventArgs);
            }
            else
            {
                foreach (var handler in eventDelegate.GetInvocationList())
                {
                    handler.DynamicInvoke(source, eventArgs);
                }
            }
        }

        internal static bool TryGetMemberPathValue(object instance, TypeInfo typeInfo, string memberPath, out object memberValue, out object parentValue, bool searchFields = true, bool searchProperties = true, Func<string, object> indexerProvider = null)
        {
            memberValue = null;
            parentValue = null;
            var splitPath = memberPath.Replace("[", ".[").Split('.').ToList();
            object next = instance;
            TypeInfo nextTypeInfo = typeInfo;
            while (instance != null && splitPath.Count > 0)
            {
                parentValue = next;
                var member = splitPath[0];
                splitPath.RemoveAt(0);
                if (!ReflectionHelpers.TryGetMemberValue(parentValue, nextTypeInfo, member, out next, searchFields, searchProperties, indexerProvider))
                {
                    memberValue = null;
                    return false;
                }
                memberValue = next;
                nextTypeInfo = next?.GetType().GetTypeInfo();
            }
            return true;
        }

        internal static bool TryGetMemberPathValue(object instance, TypeInfo typeInfo, string memberPath, out object memberValue, bool searchFields = true, bool searchProperties = true, Func<string, object> indexerProvider = null)
        {
            return TryGetMemberPathValue(instance, typeInfo, memberPath, out memberValue, out _, searchFields, searchProperties, indexerProvider);
        }
        
        internal static object GetParentOfSubentity(object instance, TypeInfo typeInfo, string path)
        {
            var splitPath = path.Replace("[", ".[").Split('.').ToList();
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
