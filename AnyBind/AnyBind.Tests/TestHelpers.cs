using AnyBind.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyBind.Tests
{
    static class TestHelpers
    {
        public static PropertyDependency ToPropertyDependency(this string propertyName)
        {
            return new PropertyDependency(propertyName);
        }
    }
}
