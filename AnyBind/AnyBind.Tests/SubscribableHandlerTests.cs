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

        /* Simulated dependency:
        [DependsOn("Num1", "Num2", "Class1.Num1", "Class1.Num2")]
        */
        public int Calculation => Num1 + Num2 + (Class1?.Num1 ?? 0) * (Class1?.Num2 ?? 0);

        public IEnumerable<string> SubscribableProperties => new string[] { "Num1", "Num2", "Class1", "Calculation" };
    }

    public class TestClass3 : ISubscribable
    {
        private double _Value = 0;


        /* Simulated dependency:
        [DependsOn("Half", "Value")]
        */
        public double Double
        {
            get => _Value * 2;
            set
            {
                _Value = value / 2;
                RaisePropertyChanged(new PropertyChangedEventArgs(nameof(Double)));
            }
        }

        /* Simulated dependency:
        [DependsOn("Double", "Value")]
        */
        public double Half
        {
            get => _Value / 2;
            set
            {
                _Value = value * 2;
                RaisePropertyChanged(new PropertyChangedEventArgs(nameof(Half)));
            }
        }

        /* Simulated dependency:
        [DependsOn("Double", "Half")]
        */
        public double Value
        {
            get => _Value;
            set
            {
                _Value = value;
                RaisePropertyChanged(new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        public IEnumerable<string> SubscribableProperties => new string[] { "Double", "Half", "Value" };

        public event PropertyChangedEventHandler PropertyChanged;

        public object GetPropertyValue(string propertyName)
        {
            switch (propertyName)
            {
                case "Double":
                    return Double;
                case "Half":
                    return Half;
                case "Value":
                    return Value;
            }
            return null;
        }

        public void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }

    public class SubscribableHandlerTests
    {
        private void SetupTestClasses()
        {
            var class1Registration = new Dictionary<DependencyBase, List<string>>();
            var class2Registration = new Dictionary<DependencyBase, List<string>>();
            var class3Registration = new Dictionary<DependencyBase, List<string>>();
            
            class2Registration.Add(new PropertyDependency("Num1"), new List<string>() { "Calculation" });
            class2Registration.Add(new PropertyDependency("Num2"), new List<string>() { "Calculation" });
            class2Registration.Add(new PropertyDependency("Class1.Num1"), new List<string>() { "Calculation" });
            class2Registration.Add(new PropertyDependency("Class1.Num2"), new List<string>() { "Calculation" });

            class3Registration.Add(new PropertyDependency("Double"), new List<string>() { "Half", "Value" });
            class3Registration.Add(new PropertyDependency("Half"), new List<string>() { "Double", "Value" });
            class3Registration.Add(new PropertyDependency("Value"), new List<string>() { "Half", "Double" });

            DependencyManager.Registrations.TryAdd(typeof(TestClass1), class1Registration);
            DependencyManager.Registrations.TryAdd(typeof(TestClass2), class2Registration);
            DependencyManager.Registrations.TryAdd(typeof(TestClass3), class3Registration);
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
        public void SubpropertyBasicTest()
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

        [Fact]
        public void SubpropertyUnsubscribeTest()
        {
            // Arrange
            SetupTestClasses();

            TestClass2 testClass = new TestClass2();
            TestClass1 testClass1a = new TestClass1();
            TestClass1 testClass1b = new TestClass1();
            SubscribableHandler handler = new SubscribableHandler(testClass);

            Dictionary<string, int> callCounts = new Dictionary<string, int>();
            callCounts.Add("Num1", 0);
            callCounts.Add("Num2", 0);
            callCounts.Add("Class1", 0);
            callCounts.Add("Class1a.Num1", 0);
            callCounts.Add("Class1a.Num2", 0);
            callCounts.Add("Class1b.Num1", 0);
            callCounts.Add("Class1b.Num2", 0);
            callCounts.Add("Calculation", 0);

            int calculation = 0;

            testClass.PropertyChanged += (s, e) =>
            {
                callCounts[e.PropertyName]++;
                calculation = testClass.Calculation;
            };

            testClass1a.PropertyChanged += (s, e) =>
            {
                callCounts[$"Class1a.{e.PropertyName}"]++;
            };

            testClass1b.PropertyChanged += (s, e) =>
            {
                callCounts[$"Class1b.{e.PropertyName}"]++;
            };

            // Act
            testClass.Num1 = 7;
            testClass.Num2 = 14;
            testClass.Class1 = testClass1a;
            testClass1a.Num1 = 2;
            testClass1a.Num2 = 5;
            testClass.Class1 = testClass1b;
            testClass1a.Num1 = 4;
            testClass1a.Num2 = 8;
            testClass1b.Num1 = 3;
            testClass1b.Num2 = 7;

            // Assert
            Assert.Equal(expected: 1, actual: callCounts["Num1"]);
            Assert.Equal(expected: 1, actual: callCounts["Num2"]);
            Assert.Equal(expected: 2, actual: callCounts["Class1"]);
            Assert.Equal(expected: 2, actual: callCounts["Class1a.Num1"]);
            Assert.Equal(expected: 2, actual: callCounts["Class1a.Num2"]);
            Assert.Equal(expected: 1, actual: callCounts["Class1b.Num1"]);
            Assert.Equal(expected: 1, actual: callCounts["Class1b.Num2"]);
            Assert.Equal(expected: 8, actual: callCounts["Calculation"]);
            Assert.Equal(expected: 42, actual: calculation);
        }

        [Fact]
        public void CircularDependencies()
        {
            // Arrange
            SetupTestClasses();

            TestClass3 testClass = new TestClass3();
            var handler = new SubscribableHandler(testClass);

            Dictionary<string, int> callCounts = new Dictionary<string, int>();
            callCounts.Add("Double", 0);
            callCounts.Add("Half", 0);
            callCounts.Add("Value", 0);

            testClass.PropertyChanged += (s, e) =>
            {
                callCounts[e.PropertyName]++;
            };

            // Act
            testClass.Double = 32;
            testClass.Half = 16;
            testClass.Value = 10;
            
            // Assert
            Assert.Equal(expected: 3, actual: callCounts["Double"]);
            Assert.Equal(expected: 3, actual: callCounts["Half"]);
            Assert.Equal(expected: 3, actual: callCounts["Value"]);
        }
    }
}
