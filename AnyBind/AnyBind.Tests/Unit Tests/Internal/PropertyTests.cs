using AnyBind.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AnyBind.Tests.Unit_Tests.Internal
{
    public class PropertyTests
    {
        [Theory]
        [InlineData("Test", true)]
        [InlineData("test", true)]
        [InlineData("_Test", true)]
        [InlineData("5Test", false)]
        [InlineData("Te5t", true)]
        [InlineData("Te.st", false)]
        [InlineData("Te()t", false)]
        public void IsValidNamedProperty(string property, bool expected)
        {
            // Arrange, Act
            var result = PropertyBase.IsValidNamedProperty(property);

            // Assert
            Assert.Equal(expected: expected, actual: result);
        }
    }
}
