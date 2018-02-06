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
        }

        class TestClass2 : TestClass1
        {
            public int Prop2 { get; set; } = 3;
            private int Field2 = 7;
            public TestClass1 Class1 = new TestClass1();
        }

        [Fact]
        public void TryGetMemberValue_Succeess()
        {
            var class2 = new TestClass2();
            object result;
            ReflectionHelpers.TryGetMemberValue(class2, typeof(TestClass2).GetTypeInfo(), "Prop1", out result);
            ReflectionHelpers.TryGetMemberValue(class2, typeof(TestClass2).GetTypeInfo(), "Prop2", out result);
            ReflectionHelpers.TryGetMemberValue(class2, typeof(TestClass2).GetTypeInfo(), "Field1", out result);
            ReflectionHelpers.TryGetMemberValue(class2, typeof(TestClass2).GetTypeInfo(), "Field2", out result);

            var parent = BindingManager.GetParent(class2, typeof(TestClass2).GetTypeInfo(), "Class1.Field1");
        }
    }
}
