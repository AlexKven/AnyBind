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

        public IEnumerable<string> SubscribableProperties => throw new NotImplementedException();
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

        [DependsOn("Num1", "Num2")]
        public int Calculation => Num1 + Num2;

        public IEnumerable<string> SubscribableProperties => throw new NotImplementedException();
    }

    public class SubscribableHandlerTests
    {
        private void SetupTestClasses()
        {
            var class1Registration = new Dictionary<DependencyBase, List<string>>();
            var class2Registration = new Dictionary<DependencyBase, List<string>>();
            
            class2Registration.Add(new PropertyDependency("Num1"), new List<string>() { "Calculation" });
            class2Registration.Add(new PropertyDependency("Num2"), new List<string>() { "Calculation" });

            DependencyManager.Registrations.TryAdd(typeof(TestClass1), class1Registration);
            DependencyManager.Registrations.TryAdd(typeof(TestClass2), class2Registration);
        }

        [Fact]
        public void BasicTest()
        {
            SetupTestClasses();

            TestClass2 testClass = new TestClass2();
            SubscribableHandler handler = new SubscribableHandler(testClass);

            testClass.PropertyChanged += (s, e) =>
            {

            };

            testClass.Num1 = 7;
        }
    }
}
