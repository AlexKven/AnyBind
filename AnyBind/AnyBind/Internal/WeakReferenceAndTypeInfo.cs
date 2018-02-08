using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AnyBind.Internal
{
    internal class WeakReferenceAndTypeInfo
    {
        public WeakReference Reference { get; }
        public TypeInfo TypeInfo { get; }
        public WeakReferenceAndTypeInfo(WeakReference reference, TypeInfo typeInfo)
        {
            Reference = reference;
            TypeInfo = typeInfo;
        }
    }
}
