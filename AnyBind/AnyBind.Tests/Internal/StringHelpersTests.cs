using AnyBind.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AnyBind.Tests.Internal
{
    public class StringHelpersTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("Single", "Single")]
        [InlineData("First.Second.Third", "First", "Second", "Third")]
        [InlineData("First.Second.Third.", "First", "Second", "Third")]
        [InlineData("First..Second.Third.", "First", "Second", "Third")]
        [InlineData(".First.Second.Third", "First", "Second", "Third")]
        [InlineData("[Index].Property", "[Index]", "Property")]
        [InlineData("Property[Index]", "Property", "[Index]")]
        [InlineData("Property[Index].Property", "Property", "[Index]", "Property")]
        [InlineData("Property.[Index].Property", "Property", "[Index]", "Property")]
        [InlineData("Property[<P1.P2>].Property", "Property", "[<P1.P2>]", "Property")]
        [InlineData("Property[<P1[inside].P2>].Property", "Property", "[<P1[inside].P2>]", "Property")]
        public void DisassemblePropertyPath(string propertyPath, params string[] components)
        {
            // Arrange, Act
            var result = propertyPath.DisassemblePropertyPath();

            // Assert
            if (components == null)
                Assert.True(result == null || result.Count() == 0);
            else
                Assert.True(components.SequenceEqual(result));
        }

        [Theory]
        [InlineData("")]
        [InlineData("Single", "Single")]
        [InlineData("First.Second.Third", "First", "Second", "Third")]
        [InlineData("[Index].Property", "[Index]", "Property")]
        [InlineData("Property[Index]", "Property", "[Index]")]
        [InlineData("Property[Index].Property", "Property", "[Index]", "Property")]
        [InlineData("Property[<P1.P2>].Property", "Property", "[<P1.P2>]", "Property")]
        public void ReassemblePropertyPath(string propertyPath, params string[] components)
        {
            // Arrange, Act
            var result = components.ReassemblePropertyPath();

            // Assert
            if (components == null)
                Assert.True(propertyPath == null || propertyPath.Length == 0);
            else
                Assert.Equal(expected: propertyPath, actual: result);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "Single", "Single")]
        [InlineData("First.Second", "Third", "First", "Second", "Third")]
        [InlineData("[Index]", "Property", "[Index]", "Property")]
        [InlineData("Property", "[Index]", "Property", "[Index]")]
        [InlineData("Property[Index]", "Property", "Property", "[Index]", "Property")]
        [InlineData("Property[<P1.P2>]", "Property", "Property", "[<P1.P2>]", "Property")]
        public void GetParentOfPropertyPath(string objectPath, string propertyName, params string[] components)
        {
            // Arrange, Act
            var result = components.GetParentOfPropertyPath(out var propertyNameResult);

            // Assert
            if (components == null || components.Length == 0)
            {
                Assert.True(objectPath == null || objectPath.Length == 0);
                Assert.True(propertyName == null || propertyName.Length == 0);
            }
            else
            {
                Assert.Equal(expected: propertyName, actual: propertyNameResult);
                Assert.Equal(expected: objectPath, actual: result);
            }
        }
    }
}
