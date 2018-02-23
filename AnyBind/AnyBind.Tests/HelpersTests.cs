using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AnyBind.Tests
{
    public class HelpersTests
    {
        [Theory]
        [InlineData(new int[] { 2 }, 1, new int[] { 1, 2 })]
        [InlineData(new int[] { }, 2, new int[] { 1, 2 })]
        [InlineData(new int[] { 3 }, 2, new int[] { 1, 2 }, new int[] { 2, 3 })]
        [InlineData(new int[] { 2, 3 }, 1, new int[] { 1, 2 }, new int[] { 2, 3 })]
        [InlineData(new int[] { 2, 3, 4 }, 1, new int[] { 1, 2 }, new int[] { 2, 3 }, new int[] { 3, 4 })]
        [InlineData(new int[] { 1, 2, 3, 5, 8, 13 }, 1, new int[] { 1, 1, 2 }, new int[] { 2, 3, 5 }, new int[] { 3, 5, 8 }, new int[] { 5, 8, 13 })]
        [InlineData(new int[] { 1, 2, 3 }, 1, new int[] { 1, 2 }, new int[] { 2, 3 }, new int[] { 3, 1 })]
        [InlineData(new int[] { 1, 2, 3 }, 1, new int[] { 2, 1 }, new int[] { 1, 2, 3 })]
        [InlineData(new int[] { 1, 2, 3 }, 1, new int[] { 1, 2, 3 }, new int[] { 2, 1, 3 }, new int[] { 3, 1, 2 })]
        [InlineData(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 1, new int[] { 1, 2, 3, 4 }, new int[] { 2, 1, 3, 5 }, new int[] { 3, 1, 2, 6 }, new int[] { 6, 7, 8, 9 })]
        void FindDependencyBranchesTests(int[] expected, int search, params int[][] nodes)
        {
            // Arrange
            Dictionary<int, List<int>> tree = new Dictionary<int, List<int>>();
            foreach (var nodeParam in nodes)
            {
                List<int> node = new List<int>();
                for (int i = 1; i < nodeParam.Length; i++)
                {
                    node.Add(nodeParam[i]);
                }
                tree.Add(nodeParam[0], node);
            }

            // Act
            var result = tree.FindDependencyBranches(search);

            // Assert
            var expectedSorted = expected.ToList();
            expectedSorted.Sort();

            result.Sort();

            Assert.True(result.SequenceEqual(expectedSorted));
        }
    }
}
