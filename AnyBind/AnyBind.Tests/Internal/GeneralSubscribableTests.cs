using AnyBind.Internal;
using System;
using System.Collections.Generic;
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

        public class Test2
        {
            public Test1 T1 { get; set; } = new Test1();
            public string Str1 { get; set; } = "One";
            public string Str2 { get; set; } = "Two";
            public string Str3 { get; set; } = "Three";
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
    }
}
