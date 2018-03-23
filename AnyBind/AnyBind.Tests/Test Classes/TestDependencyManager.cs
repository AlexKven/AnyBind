using AnyBind.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyBind.Tests.TestClasses
{
    public class TestDependencyManager : DependencyManager
    {


        public void AddPreRegistration(Type type, string dependency, string dependsOn, Type dependsOnType)
        {
            Dictionary<DependencyBase, Dictionary<string, Type>> registration;
            if (!PreRegistrations.TryGetValue(type, out registration))
            {
                registration = new Dictionary<DependencyBase, Dictionary<string, Type>>();
                PreRegistrations.Add(type, registration);
            }

            var dependencyRegistration = registration.FirstOrDefault
                (kvp => (kvp.Key as PropertyDependency)?.PropertyName == dependency).Value;
            if (dependencyRegistration == null)
            {
                dependencyRegistration = new Dictionary<string, Type>();
                registration.Add(new PropertyDependency(dependency), dependencyRegistration);
            }

            if (dependencyRegistration.ContainsKey(dependsOn))
                dependencyRegistration[dependsOn] = dependsOnType;
            else
                dependencyRegistration.Add(dependsOn, dependsOnType);

        }

        internal virtual Dictionary<DependencyBase, Dictionary<string, Type>> GetPreRegistrations(Type type)
         => PreRegistrations[type];
    }
}
