using System;
using System.Collections.Generic;
using System.Text;

namespace AnyBind
{
    public abstract class DependencyBase
    {
        public abstract bool TryHookHandler(object applyTo);
        
    }
}
