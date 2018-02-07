using System;
using System.Collections.Generic;
using System.Text;

namespace AnyBind.Internal
{
    internal abstract class DependencyBase
    {
        public abstract bool TryHookHandler(object applyTo);
    }
}
