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

        [Theory]
        [InlineData("5Test")]
        [InlineData("Te.st")]
        public void NamedProperty_ThrowsOnBadName(string propertyName)
        {
            // AAA
            Assert.Throws<ArgumentException>(() => new NamedProperty(propertyName));
        }

        [Theory]
        [InlineData("Test")]
        [InlineData("_Test")]
        [InlineData("Test5")]
        public void NamedProperty_Created(string propertyName)
        {
            // Arrange, Act
            var property = new NamedProperty(propertyName);

            // Assert
            Assert.True(property.NeedsDelimiter);
            Assert.Equal(expected: propertyName, actual: property.ToString());
        }

        [Theory]
        [InlineData("Test1", "Test1", true)]
        [InlineData("test1", "Test1", false)]
        [InlineData("Test1", "null", false)]
        public void NamedProperty_Equals(string name1, string name2, bool expected)
        {
            // Arrange
            NamedProperty prop1 = new NamedProperty(name1);
            NamedProperty prop2;
            if (name2 == null)
                prop2 = null;
            else
                prop2 = new NamedProperty(name2);

            // Act
            var result = prop1.Equals(prop2);

            // Assert
            Assert.Equal(expected: expected, actual: result);
        }

        [Theory]
        [InlineData("[0]", "0")]
        [InlineData("[1,1,2,3,5,8]", "1", "1", "2", "3", "5", "8")]
        [InlineData("[7,\"Eight\",<Numbers.Nine>]", "7", "\"Eight\"", "<Numbers.Nine>")]
        public void IndexedProperty_Created(string expectedName, params string[] indices)
        {
            // Arrange, Act
            var property = new IndexedProperty(indices);

            // Assert
            Assert.False(property.NeedsDelimiter);
            Assert.Equal(expected: expectedName, actual: property.ToString());
        }

        [Fact]
        public void IndexedProperty_ThrowsOnNoIndices()
        {
            // AAA
            Assert.Throws<ArgumentException>(() => new IndexedProperty());
        }

        [Theory]
        [InlineData(true, "1", "1")]
        [InlineData(true, "1", "1", "\"Two\"", "\"Two\"")]
        [InlineData(false, "1", "\"1\"")]
        [InlineData(false, "1", "2", "1")]
        [InlineData(true, "1", "1", "2", "2")]
        public void IndexedProperty_Equals(bool expected, params string[] indices1n2)
        {
            // Arrange
            List<string>[] indices = new List<string>[] { new List<string>(), new List<string>() };
            for (int i = 0; i < indices1n2.Length; i++)
            {
                indices[i % 2].Add(indices1n2[i]);
            }

            var prop1 = new IndexedProperty(indices[0].ToArray());
            var prop2 = new IndexedProperty(indices[1].ToArray());

            // Act
            var result = prop1.Equals(prop2);

            // Assert
            Assert.Equal(expected: expected, actual: result);
        }
    }
}
