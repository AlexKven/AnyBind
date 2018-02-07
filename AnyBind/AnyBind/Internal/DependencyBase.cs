using System;
using System.Collections.Generic;
using System.Text;

namespace AnyBind
{
    internal abstract class DependencyBase
    {
        public abstract bool TryHookHandler(object applyTo);
        
    }
}
