using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyBind.Tests
{
    public class TestDependencyManager : DependencyManager
    {

        public Dictionary<DependencyBase, List<string>> GetRegistrations(Type type)
        {
            if (!Registrations.TryGetValue(type, out var result))
                throw new KeyNotFoundException($"No such class as {type} was registered.");
            return result;
        }

        internal virtual Dictionary<DependencyBase, Dictionary<string, Type>> GetPreRegistrations(Type type)
         => PreRegistrations[type];
    }
}
