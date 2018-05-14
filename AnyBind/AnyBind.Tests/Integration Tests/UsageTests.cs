﻿using AnyBind.Tests.TestClasses;
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

            TestViewModel1 viewModel = new TestViewModel1();
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

        [Fact]
        public void TestViewModel2()
        {
            DependencyManager manager = new DependencyManager();
            manager.RegisterClass(typeof(TestViewModel2));
            manager.RegisterClass(typeof(TestViewModel1));
            manager.FinalizeRegistrations();

            int count1 = 0;
            int count2 = 0;

            TestViewModel2 viewModel = new TestViewModel2();
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Addition")
                {
                    count1++;
                }
                if (e.PropertyName == "SumAll")
                {
                    count2++;
                }
            };
            manager.InitializeInstance(viewModel);

            viewModel.Base.FirstInt = 5;
            viewModel.Base.SecondInt = 2;
            viewModel.ToAdd = 6;
            viewModel.Base = new TestClasses.TestViewModel1();
            viewModel.Base.FirstInt = 3;
            viewModel.Base.SecondInt = 4;
            viewModel.ToAdd = 5;

            Assert.Equal(expected: 7, actual: count1);
            Assert.Equal(expected: 7, actual: count2);
        }
    }
}
