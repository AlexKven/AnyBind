using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AnyBind.Internal
{
    public class WeakReferenceTypeAssociation
    {
        public WeakReference Reference { get; }
        public TypeInfo TypeInfo { get; }
        public WeakReferenceTypeAssociation(WeakReference reference, TypeInfo typeInfo)
        {
            Reference = reference;
            TypeInfo = typeInfo;
        }
    }
}
