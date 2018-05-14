using AnyBind.Adapters;
using MvvmHelpers;
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
    public class ObservableCollectionClassAdapterTests
    {
        // Arrange for all
        ObservableCollectionClassAdapter ClassAdapter = new ObservableCollectionClassAdapter();

        [Theory]
        [InlineData(typeof(ObservableCollection<int>), true)]
        [InlineData(typeof(ObservableCollection<string>), true)]
        [InlineData(typeof(List<int>), false)]
        [InlineData(typeof(DependencyManager), false)]
        [InlineData(typeof(ObservableRangeCollection<long>), true)]
        [InlineData(typeof(ObservableCollection<>), false)]
        public void CanSubscribe(Type type, bool expectedValue)
        {
            // Act
            var result = ClassAdapter.CanSubscribe(type.GetTypeInfo());

            // Assert
            Assert.Equal(expected: expectedValue, actual: result);
        }

        [Theory]
        [InlineData(typeof(ObservableCollection<int>), typeof(ObservableCollectionInstanceAdapter<int>))]
        [InlineData(typeof(ObservableCollection<string>), typeof(ObservableCollectionInstanceAdapter<string>))]
        [InlineData(typeof(ObservableRangeCollection<int>), typeof(ObservableCollectionInstanceAdapter<int>))]
        public void CreateInstanceAdapter(Type type, Type adapterType)
        {
            // Act
            var instanceAdapter = ClassAdapter.CreateInstanceAdapter(Activator.CreateInstance(type));

            // Assert
            Assert.IsType(expectedType: adapterType, @object: instanceAdapter);
        }

        [Theory]
        [InlineData(typeof(ObservableCollection<int>), "Count*", "Length", "[]*", "[5]*", "[05]*", "[five]")]
        [InlineData(typeof(ObservableRangeCollection<string>), "Count*", "Length", "[]*", "[5]*", "[05]*", "[five]*")]
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
