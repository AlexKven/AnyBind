using AnyBind.Attributes;
using AnyBind.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AnyBind.Tests
{
    public class DependencyManagerTests
    {
        public class TestClass1
        {
            public int Int1 { get; set; }
            public int Int2 { get; set; }
            public TestClass2 Test2 { get; set; }

            [DependsOn("Int1", "Int2", "Test2.Calculation")]
            public int Calculation { get; set; }
        }

        public class TestClass2
        {
            public string Str1 { get; set; }
            public string Str2 { get; set; }

            [DependsOn("Str1", "Str2")]
            public object Calculation { get; set; }
        }
        
        [Theory]
        [InlineData(typeof(TestClass1), "Calculation", "Int1", typeof(int), "Int2", typeof(int), "Test2.Calculation", typeof(object))]
        public void RegisterClass(Type type, string propertyDependency, params object[] expectedPreRegistrations)
        {
            var manager = new DependencyManager();
            var expectedDict = new Dictionary<string, Type>();
            for (int i = 0; i < expectedPreRegistrations.Length; i += 2)
            {
                expectedDict.Add((string)expectedPreRegistrations[i], (Type)expectedPreRegistrations[i + 1]);
            }

            manager.RegisterClass(type);

            var registrations = manager.GetPreRegistrations(type);
            var dependencies = registrations[new PropertyDependency(propertyDependency)];

            Assert.Equal(expected: expectedDict.Count, actual: dependencies.Count);
            foreach (var dependency in dependencies)
            {
                Assert.Equal(expected: expectedDict[dependency.Key], actual: dependency.Value);
            }
        }
    }
}
