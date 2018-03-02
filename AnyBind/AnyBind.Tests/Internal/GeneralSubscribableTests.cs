using AnyBind.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AnyBind.Tests.Internal
{
    public class GeneralSubscribableTests
    {
        public class Test1
        {
            public int Int1 { get; set; } = 1;
            public int Int2 { get; set; } = 4;
            public int Int3 { get; set; } = 9;
            public int this[int index] => index + 1;
        }

        public class Test2 : INotifyPropertyChanged
        {
            private Test1 _T1 = new Test1();
            public Test1 T1
            {
                get => _T1;
                set
                {
                    _T1 = value;
                    OnPropertyChanged(nameof(T1));
                }
            }

            private string _Str1 = "One";
            public string Str1
            {
                get => _Str1;
                set
                {
                    _Str1 = value;
                    OnPropertyChanged(nameof(Str1));
                }
            }

            private string _Str2 = "Two";
            public string Str2
            {
                get => _Str2;
                set
                {
                    _Str2 = value;
                    OnPropertyChanged(nameof(Str2));
                }
            }

            private string _Str3 = "Three";
            public string Str3
            {
                get => _Str3;
                set
                {
                    _Str3 = value;
                    OnPropertyChanged(nameof(Str3));
                }
            }

            private void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        [Theory]
        [InlineData("Str1", typeof(string), "One")]
        [InlineData("Str2", typeof(string), "Two")]
        [InlineData("Str3", typeof(string), "Three")]
        [InlineData("T1", typeof(Test1), "AnyBind.Tests.Internal.GeneralSubscribableTests+Test1")]
        [InlineData("T1.Int1", typeof(int), "1")]
        [InlineData("T1.Int2", typeof(int), "4")]
        [InlineData("T1.Int3", typeof(int), "9")]
        [InlineData("T1[5]", typeof(int), "6")]
        [InlineData("T1[<T1.Int2>]", typeof(int), "5")]
        public void GetPropertyValue(string propertyPath, Type valueType, string value)
        {
            var gs = new GeneralSubscribable(new Test2());

            var result = gs.GetPropertyValue(propertyPath);

            Assert.Equal(expected: valueType, actual: result?.GetType());
            Assert.Equal(expected: value, actual: result?.ToString());
        }

        [Theory]
        [InlineData("Str1", "S1")]
        [InlineData("Str2", "S1")]
        [InlineData("Str3", "S1")]
        [InlineData("T1", null)]
        public void PropertyChanged(string propertyName, object value)
        {
            var t2 = new Test2();
            var gs = new GeneralSubscribable(t2);
            bool raised = false;

            gs.PropertyChanged += (s, e) =>
            {
                raised = (e.PropertyName == propertyName);
            };

            gs.InstanceTypeInfo.GetProperty(propertyName).SetValue(t2, value);

            Assert.True(raised);
        }
    }
}
