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
            [DependsOn("Int1", "Test2.Str2")]
            public int Calculation2 { get; set; }
        }

        public class TestClass2
        {
            public string Str1 { get; set; }
            public string Str2 { get; set; }

            [DependsOn("Str1", "Str2")]
            public object Calculation { get; set; }
        }

        private void SetupPreRegistrations(TestDependencyManager manager, Type type)
        {
            switch (type.Name)
            {
                case "TestClass1":
                    manager.AddPreRegistration(type, "Calculation", "Int1", typeof(int));
                    manager.AddPreRegistration(type, "Calculation", "Int2", typeof(int));
                    manager.AddPreRegistration(type, "Calculation", "Test2.Calculation", typeof(object));
                    manager.AddPreRegistration(type, "Calculation2", "Int1", typeof(int));
                    manager.AddPreRegistration(type, "Calculation2", "Test2.Str2", typeof(string));
                    break;
                case "TestClass2":
                    manager.AddPreRegistration(type, "Calculation", "Str1", typeof(string));
                    manager.AddPreRegistration(type, "Calculation", "Str2", typeof(string));
                    break;
            }
        }

        [Theory]
        [InlineData(typeof(TestClass1), "Calculation", "Int1", typeof(int), "Int2", typeof(int), "Test2.Calculation", typeof(object))]
        [InlineData(typeof(TestClass1), "Calculation2", "Int1", typeof(int), "Test2.Str2", typeof(string))]
        [InlineData(typeof(TestClass2), "Calculation", "Str1", typeof(string), "Str2", typeof(string))]
        public void RegisterClass(Type type, string propertyDependency, params object[] expectedPreRegistrations)
        {
            var manager = new TestDependencyManager();
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

        [Theory]
        [InlineData(typeof(TestClass1), "Calculation", "Int1", "Int2", "Test2.Calculation")]
        [InlineData(typeof(TestClass1), "Test2.Calculation", "Test2")]
        [InlineData(typeof(TestClass2), "Calculation", "Str1", "Str2")]
        public void FinalizeRegistrations(Type type, string propertyDependency, params string[] expectedRegistrations)
        {
            var manager = new TestDependencyManager();
            SetupPreRegistrations(manager, type);

            manager.FinalizeRegistrations();

            var registrations = manager.GetRegistrations(type);
            var dependencies = registrations[new PropertyDependency(propertyDependency)];
            Assert.Equal(expected: expectedRegistrations.Length, actual: dependencies.Count);

            var expectedList = expectedRegistrations.ToList();
            foreach (var dependency in dependencies)
            {
                Assert.Contains(dependency, expectedList);
                expectedList.Remove(dependency);
            }
            Assert.Empty(expectedList);
        }
    }
}
