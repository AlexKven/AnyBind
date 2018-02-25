using AnyBind.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AnyBind.Tests
{
    public class ReflectionHelpersTests
    {
        class TestClass1
        {
            public int Prop1 { get; set; } = 5;
            private int Field1 = 10;
            public int this[int index] => index + 1;
        }

        class TestClass2 : TestClass1
        {
            public int Prop2 { get; set; } = 3;
            private int Field2 = 7;
            public TestClass1 Class1 = new TestClass1();
        }

        class TestClass3
        {
            public object Property { get; set; }
        }

        [Fact]
        public void SearchTypeAndBase()
        {
            var _this = ReflectionHelpers.SearchTypeAndBase<PropertyInfo>(typeof(TestClass2).GetTypeInfo(), t => t.DeclaredProperties.FirstOrDefault(prop => prop.Name == "Prop2"));
            var _base = ReflectionHelpers.SearchTypeAndBase<PropertyInfo>(typeof(TestClass2).GetTypeInfo(), t => t.DeclaredProperties.FirstOrDefault(prop => prop.Name == "Prop1"));
            var _foobar = ReflectionHelpers.SearchTypeAndBase<PropertyInfo>(typeof(TestClass2).GetTypeInfo(), t => t.DeclaredProperties.FirstOrDefault(prop => prop.Name == "Prop3"));
            Assert.NotNull(_this);
            Assert.NotNull(_base);
            Assert.Null(_foobar);
        }

        [Theory]
        [InlineData("Prop1", 5)]
        [InlineData("Prop2", 3)]
        [InlineData("Field1", 10)]
        [InlineData("Field2", 7)]
        [InlineData("Prop1", null, true, false)]
        [InlineData("Field1", null, false, true)]
        [InlineData("[5]", 6, false, true)]
        [InlineData("[5]", null, false, false)]
        public void TryGetMemberValue_Success(string memberName, int? value, bool fields = true, bool properties = true)
        {
            var class2 = new TestClass2();

            bool success = ReflectionHelpers.TryGetMemberValue(class2, typeof(TestClass2).GetTypeInfo(), memberName, out var result, fields, properties);

            Assert.Equal(success, value.HasValue);
            if (success)
            {
                var intResult = Assert.IsType<int>(result);
                Assert.Equal(expected: value.Value, actual: intResult);
            }
        }

        [Theory]
        [InlineData("0", 1)]
        [InlineData("1", 4)]
        [InlineData("2", 9)]
        [InlineData("3", null)]
        public void TryGetIndexedPropertyValue_ListInt_Success(string input, int? expected)
        {
            List<int> myList = new List<int>() { 1, 4, 9 };
            
            bool success = ReflectionHelpers.TryGetIndexedPropertyValue(myList, myList.GetType().GetTypeInfo(), input, out var result);

            Assert.Equal(success, expected.HasValue);
            if (success)
            {
                var intResult = Assert.IsType<int>(result);
                Assert.Equal(expected: expected.Value, actual: intResult);
            }
        }

        [Theory]
        [InlineData("a", 1)]
        [InlineData("b", 2)]
        [InlineData("c", 3)]
        [InlineData("d", null)]
        public void TryGetIndexedPropertyValue_DictStringSuccess(string input, int? expected)
        {
            Dictionary<string, int> myDict = new Dictionary<string, int>()
            { { "a", 1 }, { "b", 2 }, { "c", 3 } };

            bool success = ReflectionHelpers.TryGetIndexedPropertyValue(myDict, myDict.GetType().GetTypeInfo(), input, out var result);

            Assert.Equal(success, expected.HasValue);
            if (success)
            {
                var intResult = Assert.IsType<int>(result);
                Assert.Equal(expected: expected.Value, actual: intResult);
            }
        }

        [Theory]
        [InlineData("Property", true, typeof(TestClass3), typeof(TestClass3))]
        [InlineData("Property.Property", true, typeof(TestClass2), typeof(TestClass3))]
        [InlineData("Property.Property.Prop1", true, typeof(int), typeof(TestClass2))]
        [InlineData("Property.Property[1]", true, typeof(int), typeof(TestClass2))]
        [InlineData("Property.Property.foobar", false, null, typeof(TestClass2))]
        public void TryGetMemberPathValueTests(string input, bool success, Type expectedType, Type parentExpectedType)
        {
            TestClass3 tc3 = new TestClass3() { Property = new TestClass3() { Property = new TestClass2() } };

            bool successResult = ReflectionHelpers.TryGetMemberPathValue(tc3, typeof(TestClass3).GetTypeInfo(), input, out var result, out var parent, true, true);

            Assert.Equal(expected: success, actual: successResult);
            Assert.Equal(expected: expectedType, actual: result?.GetType());
            Assert.Equal(expected: parentExpectedType, actual: parent?.GetType());
        }
    }
}
