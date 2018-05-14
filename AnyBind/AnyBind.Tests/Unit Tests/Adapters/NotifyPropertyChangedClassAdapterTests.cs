using AnyBind.Adapters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AnyBind.Tests.Unit_Tests.Adapters
{
    public class NotifyPropertyChangedClassAdapterTests
    {
        // Arrange for all
        NotifyPropertyChangedClassAdapter ClassAdapter = new NotifyPropertyChangedClassAdapter();

        [Theory]
        [InlineData(typeof(TestClasses.TestViewModel1), true)]
        [InlineData(typeof(TestClasses.TestViewModel2), true)]
        [InlineData(typeof(object), false)]
        [InlineData(typeof(List<int>), false)]
        public void CanSubscribe(Type type, bool expectedValue)
        {
            // Act
            var result = ClassAdapter.CanSubscribe(type.GetTypeInfo());

            // Assert
            Assert.Equal(expected: expectedValue, actual: result);
        }

        [Fact]
        public void CreateInstanceAdapter()
        {
            // Act
            var instanceAdapter = ClassAdapter.CreateInstanceAdapter(new TestClasses.TestViewModel1());

            // Assert
            var adapter = Assert.IsType<NotifyPropertyChangedInstanceAdapter>(instanceAdapter);
        }

        [Theory]
        [InlineData(typeof(TestClasses.TestViewModel1), "Prop1*", "Prop2*", "[Indexed]*", "[]*")]
        public void FilterSubscribableProperties(Type type, params string[] propertyNames)
        {
            // Arrange
            (string, bool) stringIncluded(string str)
            {
                if (str.EndsWith("*"))
                    return (str.Substring(0, str.Length - 1), true);
                return (str, false);
            }

            var input = propertyNames.Select(prop => stringIncluded(prop).Item1);
            var expectedOutput = propertyNames.Select(prop => stringIncluded(prop))
                .Where(obj => obj.Item2)
                .Select(obj => obj.Item1);

            // Act
            var result = ClassAdapter.FilterSubscribableProperties(type.GetTypeInfo(), input);

            // Assert
            Assert.True(result.SequenceEqual(expectedOutput));
        }
    }
}
