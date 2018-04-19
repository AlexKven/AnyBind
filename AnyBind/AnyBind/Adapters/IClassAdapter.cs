using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AnyBind.Adapters
{
    public interface IClassAdapter
    {
        bool CanSubscribe(TypeInfo typeInfo);
        IEnumerable<string> FilterSubscribableProperties(TypeInfo typeInfo, IEnumerable<string> properties);
    }
}
