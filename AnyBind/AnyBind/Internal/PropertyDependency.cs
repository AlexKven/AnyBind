using System;
using System.Collections.Generic;
using System.Text;

namespace AnyBind.Internal
{
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
}
