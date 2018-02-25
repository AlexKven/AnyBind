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
        }

        public class Test2
        {
            public Test1 T1 { get; set; } = new Test1();
            public string Str1 { get; set; } = "One";
            public string Str2 { get; set; } = "Two";
            public string Str3 { get; set; } = "Three";
        }

        [Fact]
        public void SimpleCase()
        {
            var gs = new GeneralSubscribable(new Test2());
        }
    }
}
