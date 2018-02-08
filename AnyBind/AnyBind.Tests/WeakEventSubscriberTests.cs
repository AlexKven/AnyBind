using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace AnyBind.Tests
{
	public class WeakEventSubscriberTests
	{
		public class TestClass1
		{
			public event Action<int, string> MyEvent;

			public void OnMyEvent(int num, string str)
			{
				MyEvent?.Invoke(num, str);
			}

			public event System.ComponentModel.PropertyChangedEventHandler FakePropertyChangedEvent;

			public void OnFakePropertyChanged(string propertyName)
			{
				FakePropertyChangedEvent?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}

			public event Action<byte, short, int, long, string, bool, DateTime, dynamic> EightParameterEvent;

			public void OnEightParameterEvent(byte p1, short p2, int p3, long p4, string p5, bool p6, DateTime p7, dynamic p8)
			{
				EightParameterEvent?.Invoke(p1, p2, p3, p4, p5, p6, p7, p8);
			}
		}

		public class TestClass2
		{
			public virtual void DoSomethingSpecific(int num, string str)
			{
			}

			public virtual void DoSomethingGeneral(object[] parameters)
			{

			}
		}

		[Fact]
		public void SimpleCase()
		{
			TestClass1 tc1 = new TestClass1();
			Mock<TestClass2> tc2 = new Mock<TestClass2>() { CallBase = true };

			var eventInfo = typeof(TestClass1).GetTypeInfo().GetDeclaredEvent("MyEvent");

			WeakEventSubscriber specificSubscriber = new WeakEventSubscriber(tc2.Object, (target, parameters) => ((TestClass2)target).DoSomethingSpecific((int)parameters[0], (string)parameters[1]));
			specificSubscriber.Subscribe(eventInfo, tc1);

			tc1.OnMyEvent(5, "five");

			specificSubscriber.Unsubscribe(eventInfo, tc1);

			tc1.OnMyEvent(6, "six");

			tc2.Verify(test => test.DoSomethingSpecific(5, "five"), Times.Once);
			tc2.Verify(test => test.DoSomethingSpecific(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
		}

		[Fact]
		public void WeakReferenceHoldingTests()
		{
			TestClass1 tc1 = new TestClass1();
			TestClass2 tc2 = new TestClass2();

			WeakReference<TestClass2> tc2ref = new WeakReference<TestClass2>(tc2);
			WeakReference<TestClass1> tc1ref = new WeakReference<TestClass1>(tc1);

			var eventInfo = typeof(TestClass1).GetTypeInfo().GetDeclaredEvent("MyEvent");

			WeakEventSubscriber specificSubscriber = new WeakEventSubscriber(tc2, (target, parameters) => ((TestClass2)target).DoSomethingSpecific((int)parameters[0], (string)parameters[1]));
			specificSubscriber.Subscribe(eventInfo, tc1);

			tc1.OnMyEvent(6, "six");

			tc2 = null;

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.False(tc2ref.TryGetTarget(out tc2));
			tc1.OnMyEvent(6, "six");

			tc1 = null;

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.False(tc1ref.TryGetTarget(out tc1));
		}
	}
}
