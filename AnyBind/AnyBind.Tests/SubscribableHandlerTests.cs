using AnyBind.Attributes;
using AnyBind.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AnyBind.Tests
{
    public class TestClass1 : ISubscribable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public object GetPropertyValue(string propertyName)
        {
            switch (propertyName)
            {
                case "Num1":
                    return Num1;
                case "Num2":
                    return Num2;
            }
            return null;
        }

        private int _Num1 = 0;
        public int Num1
        {
            get => _Num1;
            set
            {
                _Num1 = value;
                OnPropertyChanged(nameof(Num1));
            }
        }

        private int _Num2 = 0;
        public int Num2
        {
            get => _Num2;
            set
            {
                _Num2 = value;
                OnPropertyChanged(nameof(Num2));
            }
        }

        public IEnumerable<string> SubscribableProperties => new string[] { "Num1", "Num2" };
}

    public class TestClass2 : ISubscribable
    {
        public TestClass2()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public object GetPropertyValue(string propertyName)
        {
            switch (propertyName)
            {
                case "Num1":
                    return Num1;
                case "Num2":
                    return Num2;
                case "Class1":
                    return Class1;
                case "Calculation":
                    return Calculation;
            }
            return null;
        }

        private int _Num1 = 2;
        public int Num1
        {
            get => _Num1;
            set
            {
                _Num1 = value;
                OnPropertyChanged(nameof(Num1));
            }
        }

        private int _Num2 = 3;
        public int Num2
        {
            get => _Num2;
            set
            {
                _Num2 = value;
                OnPropertyChanged(nameof(Num2));
            }
        }

        private TestClass1 _Class1 = null;
        public TestClass1 Class1
        {
            get => _Class1;
            set
            {
                _Class1 = value;
                OnPropertyChanged(nameof(Class1));
            }
        }

        [DependsOn("Num1", "Num2", "Class1.Num1", "Class1.Num2")]
        public int Calculation => Num1 + Num2 + (Class1?.Num1 ?? 0) * (Class1?.Num2 ?? 0);

        public IEnumerable<string> SubscribableProperties => new string[] { "Num1", "Num2", "Class1", "Calculation" };
    }

    public class SubscribableHandlerTests
    {
        private void SetupTestClasses()
        {
            var class1Registration = new Dictionary<DependencyBase, List<string>>();
            var class2Registration = new Dictionary<DependencyBase, List<string>>();
            
            class2Registration.Add(new PropertyDependency("Num1"), new List<string>() { "Calculation" });
            class2Registration.Add(new PropertyDependency("Num2"), new List<string>() { "Calculation" });
            class2Registration.Add(new PropertyDependency("Class1.Num1"), new List<string>() { "Calculation" });
            class2Registration.Add(new PropertyDependency("Class1.Num2"), new List<string>() { "Calculation" });

            DependencyManager.Registrations.TryAdd(typeof(TestClass1), class1Registration);
            DependencyManager.Registrations.TryAdd(typeof(TestClass2), class2Registration);
        }

        [Fact]
        public void BasicTest()
        {
            // Arrange
            SetupTestClasses();

            TestClass2 testClass = new TestClass2();
            SubscribableHandler handler = new SubscribableHandler(testClass);

            Dictionary<string, int> callCounts = new Dictionary<string, int>();
            callCounts.Add("Num1", 0);
            callCounts.Add("Num2", 0);
            callCounts.Add("Calculation", 0);

            int calculation = 0;

            testClass.PropertyChanged += (s, e) =>
            {
                callCounts[e.PropertyName]++;
                calculation = testClass.Calculation;
            };

            // Act
            testClass.Num1 = 7;
            testClass.Num2 = 14;

            // Assert
            Assert.Equal(expected: 1, actual: callCounts["Num1"]);
            Assert.Equal(expected: 1, actual: callCounts["Num2"]);
            Assert.Equal(expected: 2, actual: callCounts["Calculation"]);
            Assert.Equal(expected: 21, actual: calculation);
        }

        [Fact]
        public void SubclassBasicTest()
        {
            // Arrange
            SetupTestClasses();

            TestClass2 testClass = new TestClass2();
            TestClass1 testClass1 = new TestClass1();
            SubscribableHandler handler = new SubscribableHandler(testClass);

            Dictionary<string, int> callCounts = new Dictionary<string, int>();
            callCounts.Add("Num1", 0);
            callCounts.Add("Num2", 0);
            callCounts.Add("Class1", 0);
            callCounts.Add("Class1.Num1", 0);
            callCounts.Add("Class1.Num2", 0);
            callCounts.Add("Calculation", 0);

            int calculation = 0;

            testClass.PropertyChanged += (s, e) =>
            {
                callCounts[e.PropertyName]++;
                calculation = testClass.Calculation;
            };

            testClass1.PropertyChanged += (s, e) =>
            {
                callCounts[$"Class1.{e.PropertyName}"]++;
            };

            // Act
            testClass.Num1 = 7;
            testClass.Num2 = 14;
            testClass.Class1 = testClass1;
            testClass1.Num1 = 2;
            testClass1.Num2 = 5;

            // Assert
            Assert.Equal(expected: 1, actual: callCounts["Num1"]);
            Assert.Equal(expected: 1, actual: callCounts["Num2"]);
            Assert.Equal(expected: 1, actual: callCounts["Class1"]);
            Assert.Equal(expected: 1, actual: callCounts["Class1.Num1"]);
            Assert.Equal(expected: 1, actual: callCounts["Class1.Num2"]);
            Assert.Equal(expected: 5, actual: callCounts["Calculation"]);
            Assert.Equal(expected: 31, actual: calculation);
        }
    }
}
