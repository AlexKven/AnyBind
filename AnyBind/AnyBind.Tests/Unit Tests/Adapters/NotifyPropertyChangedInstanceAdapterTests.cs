using AnyBind.Adapters;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AnyBind.Tests.Unit_Tests.Adapters
{
    public class NotifyPropertyChangedInstanceAdapterTests
    {
        Mock<INotifyPropertyChanged> MockObject = new Mock<INotifyPropertyChanged>();
        private NotifyPropertyChangedInstanceAdapter Adapter;

        public NotifyPropertyChangedInstanceAdapterTests()
        {
            Adapter = new NotifyPropertyChangedInstanceAdapter(MockObject.Object);
        }

        [Theory]
        [InlineData()]
        [InlineData("One")]
        [InlineData("One", "Two", "Three")]
        public void NotifyPropertyChangedInstanceAdapter_Success_SubscribeProperties(params string[] propertyNames)
        {
            // Arrange, Act
            var result = Adapter.SubscribeToProperties(propertyNames);

            // Assert
            Assert.True(result.SequenceEqual(propertyNames));
        }

        [Fact]
        public void NotifyPropertyChangedInstanceAdapter_AdapterEventRaised()
        {
            // Arrange
            var raisedCount = 0;
            Adapter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Property")
                    raisedCount++;
            };
            Adapter.SubscribeToProperties("Property");

            // Act
            MockObject.Raise(o => o.PropertyChanged += null, new PropertyChangedEventArgs("Property"));

            // Assert
            Assert.Equal(expected: 1, actual: raisedCount);
        }

        [Fact]
        public void NotifyPropertyChangedInstanceAdapter_AdapterEventRaised_Unsubscribed()
        {
            // Arrange
            var raisedCount = 0;
            Adapter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Property")
                    raisedCount++;
            };
            Adapter.SubscribeToProperties("Property");

            // Act
            MockObject.Raise(o => o.PropertyChanged += null, new PropertyChangedEventArgs("Property"));
            Adapter.UnsubscribeFromProperties("Property");
            MockObject.Raise(o => o.PropertyChanged += null, new PropertyChangedEventArgs("Property"));

            // Assert
            Assert.Equal(expected: 1, actual: raisedCount);
        }
    }
}
