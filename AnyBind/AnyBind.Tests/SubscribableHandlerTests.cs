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

        private Dictionary<string, TestClass2> Dict = new Dictionary<string, TestClass2>()
        { { "One", new TestClass2() { Num1 = 1, Num2 = 1 } }, { "Two", new TestClass2() { Num2 = 2, Num1 = 2 } } };

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public TestClass2 this[string key]
        {
            get => Dict[key];
            set
            {
                OnPropertyChanged($"[\"{key}\"]");
            }
        }

        public object GetPropertyValue(string propertyName)
        {
            switch (propertyName)
            {
                case "Num1":
                    return Num1;
                case "Num2":
                    return Num2;
                case "Str":
                    return Str;
                case "[\"One\"]":
                    return Dict["One"];
                case "[\"Two\"]":
                    return Dict["Two"];
                case "[<Class1.Str>]":
                    return Dict[Str];
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

        private string _Str = "One";
        public string Str
        {
            get => _Str;
            set
            {
                _Str = value;
                OnPropertyChanged(nameof(Str));
            }
        }
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
                case "Class4":
                    return Class4;
                case "Calculation":
                    return Calculation;
            }
            if (propertyName.StartsWith("Class1"))
            {
                var sub = propertyName.Substring(6);
                if (sub.StartsWith("."))
                    sub = sub.Substring(1);
                return Class1?.GetPropertyValue(sub);
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

        private TestClass4 _Class4 = null;
        public TestClass4 Class4
        {
            get => _Class4;
            set
            {
                _Class4 = value;
                OnPropertyChanged(nameof(Class4));
            }
        }

        /* Simulated dependency:
        [DependsOn("Num1", "Num2", "Class1.Num1", "Class1.Num2")]
        */
        public int Calculation => Num1 + Num2 + (Class1?.Num1 ?? 0) * (Class1?.Num2 ?? 0);

        /* Simulated dependency:
        [DependsOn("Class1[\"One\"].Num1", "Class1[\"Two\"].Num1")]
        */
        public int Indexer => Class1["One"].Num1 + Class1["Two"].Num1;

        /* Simulated dependency:
        [DependsOn("Class1[<Class1.Str>].Num2")]
        */
        public int BoundIndexer => Class1[Class1.Str].Num2;

        /* Simulated dependency:
        [DependsOn("Class4.Calculation")]
        */
        public int Calculation4 => Class4.Calculation;
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

    public class TestClass4 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        private TestClass4 _Sub;
        public TestClass4 Sub
        {
            get => _Sub;
            set
            {
                _Sub = value;
                OnPropertyChanged(nameof(Sub));
            }
        }

        /* Simulated dependency:
        [DependsOn("Num1", "Num2", "Sub.Calculation")]
        */
        public int Calculation => Num1 + Num2 + Sub?.Calculation ?? 0;
    }

    public class SubscribableHandlerTests
    {
        private void SetupTestClasses()
        {
            var class1Registration = new Dictionary<DependencyBase, List<string>>();
            var class2Registration = new Dictionary<DependencyBase, List<string>>();
            var class3Registration = new Dictionary<DependencyBase, List<string>>();
            var class4Registration = new Dictionary<DependencyBase, List<string>>();
            
            class2Registration.Add(new PropertyDependency("Num1"), new List<string>() { "Calculation" });
            class2Registration.Add(new PropertyDependency("Num2"), new List<string>() { "Calculation" });
            class2Registration.Add(new PropertyDependency("Class1.Num1"), new List<string>() { "Calculation" });
            class2Registration.Add(new PropertyDependency("Class1.Num2"), new List<string>() { "Calculation" });
            class2Registration.Add(new PropertyDependency("Class1[\"One\"].Num1"), new List<string>() { "Indexer" });
            class2Registration.Add(new PropertyDependency("Class1[\"Two\"].Num1"), new List<string>() { "Indexer" });
            class2Registration.Add(new PropertyDependency("Class1[<Class1.Str>].Num2"), new List<string>() { "BoundIndexer" });
            class2Registration.Add(new PropertyDependency("Class4.Calculation"), new List<string>() { "Calculation4" });

            // These should be implicitly added
            class2Registration.Add(new PropertyDependency("Class1"), new List<string>() { "Class1.Num2", "Class1.Num1", "Class1[<Class1.Str>]", "Class1[\"One\"]", "Class1[\"Two\"]" });
            class2Registration.Add(new PropertyDependency("Class1.Str"), new List<string>() { "Class1[<Class1.Str>]" });
            class2Registration.Add(new PropertyDependency("Class1[\"One\"]"), new List<string>() { "Class1[\"One\"].Num1" });
            class2Registration.Add(new PropertyDependency("Class1[\"Two\"]"), new List<string>() { "Class1[\"Two\"].Num1" });
            class2Registration.Add(new PropertyDependency("Class1[<Class1.Str>]"), new List<string>() { "Class1[<Class1.Str>].Num2" });
            class2Registration.Add(new PropertyDependency("Class4"), new List<string>() { "Class4.Calculation" });
            class2Registration.Add(new PropertyDependency("Class4.Num1"), new List<string>() { "Class4.Calculation" });
            class2Registration.Add(new PropertyDependency("Class4.Num2"), new List<string>() { "Class4.Calculation" });
            class2Registration.Add(new PropertyDependency("Class4.Sub"), new List<string>() { "Class4.Calculation" });

            class3Registration.Add(new PropertyDependency("Double"), new List<string>() { "Half", "Value" });
            class3Registration.Add(new PropertyDependency("Half"), new List<string>() { "Double", "Value" });
            class3Registration.Add(new PropertyDependency("Value"), new List<string>() { "Half", "Double" });

            class4Registration.Add(new PropertyDependency("Num1"), new List<string>() { "Calculation" });
            class4Registration.Add(new PropertyDependency("Num2"), new List<string>() { "Calculation" });
            class4Registration.Add(new PropertyDependency("Sub.Calculation"), new List<string>() { "Calculation" });
            class4Registration.Add(new PropertyDependency("Sub"), new List<string>() { "Sub.Calculation" });

            DependencyManager.Registrations.TryAdd(typeof(TestClass1), class1Registration);
            DependencyManager.Registrations.TryAdd(typeof(TestClass2), class2Registration);
            DependencyManager.Registrations.TryAdd(typeof(TestClass3), class3Registration);
            DependencyManager.Registrations.TryAdd(typeof(TestClass4), class4Registration);
        }

        private Dictionary<string, int> GetCallCountsDict()
        {
            Dictionary<string, int> callCounts = new Dictionary<string, int>();
            callCounts.Add("Num1", 0);
            callCounts.Add("Num2", 0);
            callCounts.Add("Class1", 0);
            callCounts.Add("Class1[\"One\"].Num1", 0);
            callCounts.Add("Class1[\"Two\"].Num1", 0);
            callCounts.Add("Class1[\"One\"].Num2", 0);
            callCounts.Add("Class1[\"Two\"].Num2", 0);
            callCounts.Add("Class1[\"One\"]", 0);
            callCounts.Add("Class1[\"Two\"]", 0);
            callCounts.Add("Class1[<Class1.Str>]", 0);
            callCounts.Add("Class1[<Class1.Str>].Num2", 0);
            callCounts.Add("Class1.Num1", 0);
            callCounts.Add("Class1.Num2", 0);
            callCounts.Add("Class1a.Num1", 0);
            callCounts.Add("Class1a.Num2", 0);
            callCounts.Add("Class1b.Num1", 0);
            callCounts.Add("Class1b.Num2", 0);
            callCounts.Add("Class4", 0);
            callCounts.Add("Class4.Num1", 0);
            callCounts.Add("Class4.Num2", 0);
            callCounts.Add("Class4.Sub", 0);
            callCounts.Add("Class4.Calculation", 0);
            callCounts.Add("Indexer", 0);
            callCounts.Add("BoundIndexer", 0);
            callCounts.Add("Calculation", 0);
            callCounts.Add("Calculation4", 0);
            return callCounts;
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

            Dictionary<string, int> callCounts = GetCallCountsDict();

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

            Dictionary<string, int> callCounts = GetCallCountsDict();

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
            Assert.Equal(expected: 8, actual: callCounts["Calculation"]);
            Assert.Equal(expected: 42, actual: calculation);
        }

        [Fact]
        public void SimpleCircularDependencies()
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

        [Fact]
        public void SubpropertyIndexedTest()
        {
            // Arrange
            SetupTestClasses();

            TestClass2 testClass = new TestClass2();
            TestClass1 testClass1 = new TestClass1();
            testClass.Class1 = testClass1;
            SubscribableHandler handler = new SubscribableHandler(testClass);

            Dictionary<string, int> callCounts = GetCallCountsDict();

            int calculation = 0;

            testClass.PropertyChanged += (s, e) =>
            {
                callCounts[e.PropertyName]++;
                calculation = testClass.Indexer;
            };

            testClass1.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName.StartsWith("["))
                    callCounts[$"Class1{e.PropertyName}"]++;
                else
                    callCounts[$"Class1.{e.PropertyName}"]++;
            };

            // Act
            testClass.Class1["One"].Num1 = 2;
            testClass.Class1["One"].Num2 = 2;
            testClass.Class1["Two"].Num1 = 3;
            testClass.Class1["Two"].Num2 = 3;
            testClass.Class1 = new TestClass1();
            testClass.Class1["One"].Num2 = 2;
            testClass.Class1.Str = "Two";
            testClass.Class1["Two"].Num2 = 3;
            testClass.Class1["One"] = new TestClass2();
            testClass.Class1["Two"] = new TestClass2();
            testClass.Class1["One"].Num1 = 2;
            testClass.Class1["Two"].Num1 = 3;

            // Assert
            Assert.Equal(expected: 7, actual: callCounts["Indexer"]);
            Assert.Equal(expected: 5, actual: callCounts["BoundIndexer"]);
            Assert.Equal(expected: 5, actual: calculation);
        }

        [Fact]
        public void NonISubscribableSubPropertyTest()
        {
            // Arrange
            SetupTestClasses();

            TestClass2 testClass = new TestClass2();
            TestClass4 testClass4 = new TestClass4();
            SubscribableHandler handler = new SubscribableHandler(testClass);

            Dictionary<string, int> callCounts = GetCallCountsDict();

            int calculation4 = 0;

            testClass.PropertyChanged += (s, e) =>
            {
                callCounts[e.PropertyName]++;
                calculation4 = testClass.Calculation4;
            };

            // Act
            testClass.Class4 = testClass4;
            testClass4.Num1 = 10;
            var testClass4b = new TestClass4();
            testClass4.Sub = testClass4b;
            testClass4b.Num2 = 20;

            // Assert
            Assert.Equal(expected: 4, actual: callCounts["Calculation4"]);
            Assert.Equal(expected: 30, actual: calculation4);
        }
    }
}
