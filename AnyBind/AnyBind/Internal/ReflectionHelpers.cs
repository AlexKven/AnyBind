using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AnyBind.Internal
{
    internal static class ReflectionHelpers
    {
        internal static bool TryGetMemberValue(object instance, TypeInfo typeInfo, string memberName, out object memberValue, bool searchFields = true, bool searchProperties = true)
        {
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

        internal static bool TryGetMemberPathValue(object instance, TypeInfo typeInfo, string memberPath, out object memberValue, out object parentValue, bool searchFields = true, bool searchProperties = true)
        {
            memberValue = null;
            parentValue = null;
            var splitPath = memberPath.DisassemblePropertyPath().ToList();
            object next = instance;
            TypeInfo nextTypeInfo = typeInfo;
            while (instance != null && splitPath.Any())
            {
                parentValue = next;
                var member = splitPath[0];
                splitPath.RemoveAt(0);
                if (!ReflectionHelpers.TryGetMemberValue(parentValue, nextTypeInfo, member, out next, searchFields, searchProperties))
                {
                    memberValue = null;
                    return false;
                }
                memberValue = next;
                nextTypeInfo = next?.GetType().GetTypeInfo();
            }
            return true;
        }

        internal static bool TryGetMemberPathValue(object instance, TypeInfo typeInfo, string memberPath, out object memberValue, bool searchFields = true, bool searchProperties = true)
        {
            return TryGetMemberPathValue(instance, typeInfo, memberPath, out memberValue, out _, searchFields, searchProperties);
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

        internal static Type GetTypeOfPath(Type objectType, IEnumerable<string> pathComponents)
        {
            if (!pathComponents.Any())
                return objectType;
            if (objectType == null)
                return null;
            var nextProperty = pathComponents.First();
            var nextPath = pathComponents.Skip(1);

            PropertyInfo property;
            if (nextProperty.StartsWith("["))
                property = SearchTypeAndBase(objectType.GetTypeInfo(), t => t.DeclaredProperties.FirstOrDefault(pi => pi.Name == "Item"));
            else
                property = SearchTypeAndBase(objectType.GetTypeInfo(), t => t.DeclaredProperties.FirstOrDefault(pi => pi.Name == nextProperty));
            return GetTypeOfPath(property?.PropertyType, nextPath);
        }
    }
}
