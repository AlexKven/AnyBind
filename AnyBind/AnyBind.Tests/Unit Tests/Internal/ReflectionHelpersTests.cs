using AnyBind.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AnyBind.Tests.UnitTests.Internal
{
    public class ReflectionHelpersTests
    {
        class TestClass1
        {
            public int Prop1 { get; set; } = 5;
            private int Field1 = 10;
            public int this[int index] => index + 1;
            public TestClass3 Class3 { get; set; }
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

        private struct Bingo
        {
            public int Number { get; set; }
            public char Letter { get; set; }

            public Bingo(char letter, int number)
            {
                Number = number;
                Letter = letter;
            }

            public static Bingo Convert(int input)
            {
                var x = input % 5;
                var y = input / 5;

                var result = new Bingo();
                result.Number = x + 1;
                switch (y)
                {
                    case 0:
                        result.Letter = 'A';
                        break;
                    case 1:
                        result.Letter = 'B';
                        break;
                    case 2:
                        result.Letter = 'C';
                        break;
                    case 3:
                        result.Letter = 'D';
                        break;
                    case 4:
                        result.Letter = 'E';
                        break;
                }

                return result;
            }
        }

        [Theory]
        [InlineData("Property", true, typeof(TestClass3), typeof(TestClass3))]
        [InlineData("Property.Property", true, typeof(TestClass2), typeof(TestClass3))]
        [InlineData("Property.Property.Prop1", true, typeof(int), typeof(TestClass2), "5")]
        [InlineData("Property.Property.foobar", false, null, typeof(TestClass2))]
        public void TryGetMemberPathValueTests(string input, bool success, Type expectedType, Type parentExpectedType, string expectedValue = null)
        {
            TestClass3 tc3 = new TestClass3() { Property = new TestClass3() { Property = new TestClass2() } };

            bool successResult = ReflectionHelpers.TryGetMemberPathValue(tc3, typeof(TestClass3).GetTypeInfo(), input, out var result, out var parent, true, true);

            Assert.Equal(expected: success, actual: successResult);
            Assert.Equal(expected: expectedType, actual: result?.GetType());
            Assert.Equal(expected: parentExpectedType, actual: parent?.GetType());
            if (expectedValue != null)
                Assert.Equal(expected: expectedValue, actual: result.ToString());
        }

        [Theory]
        [InlineData(typeof(TestClass1), "Prop1", typeof(int))]
        [InlineData(typeof(TestClass1), "Field1", null)]
        [InlineData(typeof(TestClass1), "Class3", typeof(TestClass3))]
        [InlineData(typeof(TestClass1), "Class3.Property", typeof(object))]
        [InlineData(typeof(TestClass2), "Prop2", typeof(int))]
        [InlineData(typeof(TestClass3), "Property", typeof(object))]
        [InlineData(typeof(TestClass3), "", typeof(TestClass3))]
        public void GetTypeOfPath(Type originatingType, string path, Type expectedType)
        {
            var result = ReflectionHelpers.GetTypeOfPath(originatingType, path.DisassemblePropertyPath());

            Assert.Equal(expected: expectedType, actual: result);
        }
    }
}
