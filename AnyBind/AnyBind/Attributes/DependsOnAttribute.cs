using System;
using System.Collections.Generic;
using System.Text;

namespace AnyBind.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class DependsOnAttribute : Attribute
    {
        public string[] PropertyPaths { get; }
        public DependsOnAttribute(params string[] propertyPaths)
        {
            PropertyPaths = propertyPaths;
        }
    }
}
