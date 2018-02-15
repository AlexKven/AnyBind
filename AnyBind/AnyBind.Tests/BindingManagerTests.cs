using AnyBind.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AnyBind.Tests
{
    public class BindingManagerTests
    {
        public class TestClass1 : INotifyPropertyChanged
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
        }

        public class TestClass2 : INotifyPropertyChanged
        {
            public TestClass2()
            {
                BindingManager.SetupBindings(this);
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        }

        [Fact]
        public void Test()
        {
            BindingManager.RegisterClass(typeof(TestClass2));

            var class2 = new TestClass2();

            class2.PropertyChanged += Class2_PropertyChanged;

            class2.Num1 = 7;
        }

        private void Class2_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
    }
}
