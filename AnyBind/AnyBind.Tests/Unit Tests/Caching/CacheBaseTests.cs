using AnyBind.Caching;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AnyBind.Tests.UnitTests.Caching
{
    public class CacheBaseTests
    {
        CacheBase<string, int> Cache;
        private Mock<CacheBase<string, int>> MockCache = new Mock<AnyBind.Caching.CacheBase<string, int>>();
        private Dictionary<string, int> Items = new Dictionary<string, int>();
        delegate void ReturnCallback(string key, out int result);

        void Setup()
        {
            Cache = MockCache.Object;
            MockCache.Protected().Setup<bool>("TrySetValueInternal", ItExpr.IsAny<string>(), ItExpr.IsAny<int>()).Returns(true);
            MockCache.Protected().Setup<bool>("TryGetValueInternal", ItExpr.IsAny<string>(), ItExpr.Ref<int>.IsAny).Returns(true);
            MockCache.Protected().Setup<bool>("TryClearValueInternal", ItExpr.IsAny<string>(), ItExpr.Ref<int>.IsAny).Returns(true);
            MockCache.Setup(cache => cache.GetEnumerator()).Returns(Items.GetEnumerator());
        }

        void SetupGet()
        {
            foreach (var kvp in Items)
            {
                MockCache.Protected().Setup<bool>("TryGetValueInternal", kvp.Key, ItExpr.Ref<int>.IsAny).
                    Callback(new ReturnCallback((string key, out int result) => result = kvp.Value)).Returns(true);
            }
        }

        [Fact]
        public void SetActions_Success()
        {
            // Arrange
            Setup();
            var setAction = Mock.Of<Action<string, int>>();
            Cache.SetActions.Add(setAction);

            // Act
            Cache.TrySetValue("Five", 5);

            // Assert
            Mock.Get(setAction).Verify(act => act("Five", 5));
        }

        [Fact]
        public void GetActions_Success()
        {
            // Arrange
            Setup();
            var getAction = Mock.Of<Action<string, int>>();
            Cache.GetActions.Add(getAction);
            Items.Add("Five", 5);
            SetupGet();

            // Act
            Cache.TryGetValue("Five", out var result);

            // Assert
            Mock.Get(getAction).Verify(act => act("Five", 5));
            Assert.Equal(expected: 5, actual: result);
        }
    }
}
