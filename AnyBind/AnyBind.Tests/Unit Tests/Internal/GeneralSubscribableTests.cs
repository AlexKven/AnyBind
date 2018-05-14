using AnyBind.Adapters;
using AnyBind.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AnyBind.Tests.UnitTests.Internal
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

        public class Test3 : ObservableCollection<int>
        {

        }

        protected Mock<DependencyManager> DependencyManager;

        public GeneralSubscribableTests()
        {
            DependencyManager = new Mock<AnyBind.DependencyManager>()
            {
                CallBase = true
            };

            DependencyManager.Object.RegisterClass(typeof(Test1));
            DependencyManager.Object.RegisterClass(typeof(Test2));
            DependencyManager.Object.RegisterClass(typeof(Test3));
            DependencyManager.Object.FinalizeRegistrations();
        }

        [Theory]
        [InlineData("Str1", typeof(string), "One")]
        [InlineData("Str2", typeof(string), "Two")]
        [InlineData("Str3", typeof(string), "Three")]
        [InlineData("T1", typeof(Test1), "AnyBind.Tests.UnitTests.Internal.GeneralSubscribableTests+Test1")]
        [InlineData("T1.Int1", typeof(int), "1")]
        [InlineData("T1.Int2", typeof(int), "4")]
        [InlineData("T1.Int3", typeof(int), "9")]
        [InlineData("T1[5]", typeof(int), "6")]
        [InlineData("T1[<T1.Int2>]", typeof(int), "5")]
        public void GetPropertyValue(string propertyPath, Type valueType, string value)
        {
            var gs = new GeneralSubscribable(new Test2(), DependencyManager.Object);

            var result = gs.GetPropertyValue(propertyPath);

            Assert.Equal(expected: valueType, actual: result?.GetType());
            Assert.Equal(expected: value, actual: result?.ToString());
        }

        [Theory]
        [InlineData("Str1", "S1")]
        [InlineData("Str2", "S1")]
        [InlineData("Str3", "S1")]
        [InlineData("T1", null)]
        public void PropertyChanged_INotifyPropertyChanged(string propertyName, object value)
        {
            var t2 = new Test2();
            var gs = new GeneralSubscribable(t2, DependencyManager.Object);
            bool raised = false;

            gs.PropertyChanged += (s, e) =>
            {
                raised = (e.PropertyName == propertyName);
            };

            gs.InstanceTypeInfo.GetProperty(propertyName).SetValue(t2, value);

            Assert.True(raised);
        }

        [Theory]
        [InlineData("Count", true)]
        [InlineData("Length", false)]
        [InlineData("[]", true)]
        [InlineData("[0]", true)]
        [InlineData("[1]", false)]
        public void PropertyChanged_ObservableCollection_Add(string propertyName, bool expectedRaised)
        {
            var collection = new ObservableCollection<int>();
            var gs = new GeneralSubscribable(collection, DependencyManager.Object);
            bool raised = false;

            gs.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == propertyName)
                    raised = true;
            };

            collection.Add(703);

            Assert.Equal(expected: expectedRaised, actual: raised);
        }

        [Theory]
        [InlineData(typeof(Test2), true)]
        [InlineData(typeof(Test3), true)]
        [InlineData(typeof(Test1), false)]
        public void CanSubscribe(Type type, bool result)
        {
            Assert.Equal(expected: result, actual: DependencyManager.Object.CanSubscribe(type?.GetTypeInfo()));
        }
        
        /// <param name="type"></param>
        /// <param name="properties">
        /// For properites that you expect to be subscribable, pass with a '+',
        /// e.g. "PropertyName+"
        /// </param>
        [Theory]
        [InlineData(typeof(Test2), true, "Prop1+", "Prop2+")]
        [InlineData(typeof(Test3), true, "Count+", "Length+")]
        [InlineData(typeof(Test3), false, "Count+", "Length")]
        [InlineData(typeof(Test1), true, "Prop1", "Prop2")]
        public void FilterSubscribableProperties(Type type, bool includeNotifyPropertyClassAdapter, params string[] properties)
        {
            // Arrange
            if (!includeNotifyPropertyClassAdapter)
            {
                DependencyManager.Setup(dm => dm.GetClassAdapters())
                    .Returns(new List<IClassAdapter>() { new ObservableCollectionClassAdapter() });
            }
            var input = properties.Select(prop => prop.EndsWith("+") ? prop.Substring(0, prop.Length - 1) : prop);
            var expectedOutput = properties.Where(prop => prop.EndsWith("+")).Select(prop => prop.Substring(0, prop.Length - 1));

            // Act
            var output = DependencyManager.Object.FilterSubscribableProperties(type?.GetTypeInfo(), input);

            // Assert
            Assert.True(expectedOutput.SequenceEqual(output));
        }
    }
}
