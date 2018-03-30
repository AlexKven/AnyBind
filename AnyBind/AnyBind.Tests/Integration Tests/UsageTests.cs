using AnyBind.Tests.Test_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AnyBind.Tests.Integration_Tests
{
    public class UsageTests
    {
        [Fact]
        public void TestViewModel1()
        {
            DependencyManager manager = new DependencyManager();
            manager.RegisterClass(typeof(TestViewModel1));
            manager.FinalizeRegistrations();

            int count = 0;

            TestViewModel1 viewModel = new Test_Classes.TestViewModel1();
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Multiplication")
                {
                    count++;
                }
            };
            manager.InitializeInstance(viewModel);

            viewModel.FirstInt = 5;
            viewModel.SecondInt = 6;

            Assert.Equal(expected: 2, actual: count);
        }
    }
}
