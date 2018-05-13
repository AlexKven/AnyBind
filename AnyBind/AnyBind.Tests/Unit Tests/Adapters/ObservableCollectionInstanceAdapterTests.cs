using AnyBind.Adapters;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AnyBind.Tests.Unit_Tests.Adapters
{
    public class ObservableCollectionInstanceAdapterTests
    {
        ObservableRangeCollection<string> Object = new ObservableRangeCollection<string>();
        private ObservableCollectionInstanceAdapter<string> Adapter;

        public ObservableCollectionInstanceAdapterTests()
        {
            Adapter = new ObservableCollectionInstanceAdapter<string>(Object);
        }

        [Theory]
        [InlineData()]
        [InlineData("One")]
        [InlineData("One", "Count*")]
        [InlineData("Count*")]
        [InlineData("Count*", "One", "[]*")]
        [InlineData("[FooBar]")]
        [InlineData("[1]*", "[2]*")]
        [InlineData("[1]*", "[2]*", "[Foo]", "Bar")]
        public void NotifyPropertyChangedInstanceAdapter_Success_SubscribeProperties(params string[] propertyNames)
        {
            (string, bool) stringIncluded(string str)
            {
                if (str.EndsWith("*"))
                    return (str.Substring(0, str.Length - 1), true);
                return (str, false);
            }

            //Arrange
            var input = propertyNames.Select(prop => stringIncluded(prop).Item1);
            var expectedOutput = propertyNames.Select(prop => stringIncluded(prop))
                .Where(obj => obj.Item2)
                .Select(obj => obj.Item1);
            
            // Act
            var result = Adapter.SubscribeToProperties(input.ToArray());

            // Assert
            Assert.True(result.SequenceEqual(expectedOutput));
        }

        [Fact]
        public void NotifyPropertyChangedInstanceAdapter_EventsRaised_Add()
        {
            // Arrange
            Dictionary<string, int> raiseCounts = new Dictionary<string, int>()
            { ["Count"] = 0, ["[]"] = 0, ["[0]"] = 0, ["[00]"] = 0, ["[1]"] = 0 };
            Adapter.PropertyChanged += (s, e) =>
            {
                if (raiseCounts.ContainsKey(e.PropertyName))
                    raiseCounts[e.PropertyName]++;
            };

            foreach (var prop in raiseCounts.Keys)
            {
                Adapter.SubscribeToProperties(prop);
            }

            // Act
            Object.Add("Test1");

            // Assert
            Assert.Equal(expected: 1, actual: raiseCounts["Count"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[0]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[00]"]);
            Assert.Equal(expected: 0, actual: raiseCounts["[1]"]);
        }

        [Fact]
        public void NotifyPropertyChangedInstanceAdapter_EventsRaised_AddMany()
        {
            // Arrange
            Dictionary<string, int> raiseCounts = new Dictionary<string, int>()
            { ["Count"] = 0,
                ["[]"] = 0,
                ["[0]"] = 0,
                ["[2]"] = 0,
                ["[02]"] = 0,
                ["[3]"] = 0,
            };
            Adapter.PropertyChanged += (s, e) =>
            {
                if (raiseCounts.ContainsKey(e.PropertyName))
                    raiseCounts[e.PropertyName]++;
            };

            foreach (var prop in raiseCounts.Keys)
            {
                Adapter.SubscribeToProperties(prop);
            }

            // Act
            Object.AddRange(new string[] { "Test1", "Test2", "Test3" });

            // Assert
            Assert.Equal(expected: 1, actual: raiseCounts["Count"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[0]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[2]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[02]"]);
            Assert.Equal(expected: 0, actual: raiseCounts["[3]"]);
        }

        [Fact]
        public void NotifyPropertyChangedInstanceAdapter_EventsRaised_Remove_End()
        {
            // Arrange
            for (int i = 0; i < 10; i++)
            {
                Object.Add($"{i}");
            }

            Dictionary<string, int> raiseCounts = new Dictionary<string, int>()
            { ["Count"] = 0, ["[]"] = 0, ["[8]"] = 0, ["[9]"] = 0, ["[10]"] = 0 };
            Adapter.PropertyChanged += (s, e) =>
            {
                if (raiseCounts.ContainsKey(e.PropertyName))
                    raiseCounts[e.PropertyName]++;
            };

            foreach (var prop in raiseCounts.Keys)
            {
                Adapter.SubscribeToProperties(prop);
            }

            // Act
            Object.Remove("9");

            // Assert
            Assert.Equal(expected: 1, actual: raiseCounts["Count"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[]"]);
            Assert.Equal(expected: 0, actual: raiseCounts["[8]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[9]"]);
            Assert.Equal(expected: 0, actual: raiseCounts["[10]"]);
        }

        [Fact]
        public void NotifyPropertyChangedInstanceAdapter_EventsRaised_Remove_Middle()
        {
            // Arrange
            for (int i = 0; i < 10; i++)
            {
                Object.Add($"{i}");
            }

            Dictionary<string, int> raiseCounts = new Dictionary<string, int>()
            { ["Count"] = 0, ["[]"] = 0, ["[6]"] = 0, ["[7]"] = 0, ["[8]"] = 0, ["[9]"] = 0, ["[10]"] = 0 };
            Adapter.PropertyChanged += (s, e) =>
            {
                if (raiseCounts.ContainsKey(e.PropertyName))
                    raiseCounts[e.PropertyName]++;
            };

            foreach (var prop in raiseCounts.Keys)
            {
                Adapter.SubscribeToProperties(prop);
            }

            // Act
            Object.Remove("7");

            // Assert
            Assert.Equal(expected: 1, actual: raiseCounts["Count"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[]"]);
            Assert.Equal(expected: 0, actual: raiseCounts["[6]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[7]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[8]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[9]"]);
            Assert.Equal(expected: 0, actual: raiseCounts["[10]"]);
        }

        [Fact]
        public void NotifyPropertyChangedInstanceAdapter_EventsRaised_Reset()
        {
            // Arrange
            for (int i = 0; i < 10; i++)
            {
                Object.Add($"{i}");
            }

            Dictionary<string, int> raiseCounts = new Dictionary<string, int>()
            { ["Count"] = 0, ["[]"] = 0, ["[6]"] = 0, ["[3]"] = 0, ["[7]"] = 0, ["[8]"] = 0, ["[9]"] = 0, ["[10]"] = 0 };
            Adapter.PropertyChanged += (s, e) =>
            {
                if (raiseCounts.ContainsKey(e.PropertyName))
                    raiseCounts[e.PropertyName]++;
            };

            foreach (var prop in raiseCounts.Keys)
            {
                Adapter.SubscribeToProperties(prop);
            }

            // Act
            Object.RemoveRange(new string[] { "7", "8", "9" });

            // Assert
            Assert.Equal(expected: 1, actual: raiseCounts["Count"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[6]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[3]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[7]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[8]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[9]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[10]"]);
        }

        [Fact]
        public void NotifyPropertyChangedInstanceAdapter_EventsRaised_Assign()
        {
            // Arrange
            for (int i = 0; i < 10; i++)
            {
                Object.Add($"{i}");
            }

            Dictionary<string, int> raiseCounts = new Dictionary<string, int>()
            { ["Count"] = 0, ["[]"] = 0, ["[6]"] = 0, ["[7]"] = 0, ["[8]"] = 0, ["[9]"] = 0, ["[10]"] = 0 };
            Adapter.PropertyChanged += (s, e) =>
            {
                if (raiseCounts.ContainsKey(e.PropertyName))
                    raiseCounts[e.PropertyName]++;
            };

            foreach (var prop in raiseCounts.Keys)
            {
                Adapter.SubscribeToProperties(prop);
            }

            // Act
            Object[7] = "Seven";

            // Assert
            Assert.Equal(expected: 0, actual: raiseCounts["Count"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[]"]);
            Assert.Equal(expected: 0, actual: raiseCounts["[6]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[7]"]);
            Assert.Equal(expected: 0, actual: raiseCounts["[8]"]);
            Assert.Equal(expected: 0, actual: raiseCounts["[9]"]);
            Assert.Equal(expected: 0, actual: raiseCounts["[10]"]);
        }

        [Fact]
        public void NotifyPropertyChangedInstanceAdapter_EventsRaised_Move()
        {
            // Arrange
            for (int i = 0; i < 10; i++)
            {
                Object.Add($"{i}");
            }

            Dictionary<string, int> raiseCounts = new Dictionary<string, int>()
            { ["Count"] = 0, ["[]"] = 0, ["[2]"] = 0, ["[3]"] = 0, ["[4]"] = 0, ["[5]"] = 0, ["[6]"] = 0, ["[7]"] = 0 };
            Adapter.PropertyChanged += (s, e) =>
            {
                if (raiseCounts.ContainsKey(e.PropertyName))
                    raiseCounts[e.PropertyName]++;
            };

            foreach (var prop in raiseCounts.Keys)
            {
                Adapter.SubscribeToProperties(prop);
            }

            // Act
            Object.Move(3, 6);

            // Assert
            Assert.Equal(expected: 0, actual: raiseCounts["Count"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[]"]);
            Assert.Equal(expected: 0, actual: raiseCounts["[2]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[3]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[4]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[5]"]);
            Assert.Equal(expected: 1, actual: raiseCounts["[6]"]);
            Assert.Equal(expected: 0, actual: raiseCounts["[7]"]);
        }
    }
}
