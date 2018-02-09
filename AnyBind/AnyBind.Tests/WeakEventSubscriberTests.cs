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

			public event Action NoParameterEvent;

			public void OnNoParameterEvent()
			{
				NoParameterEvent?.Invoke();
			}
		}

		public class TestClass2
		{
			public virtual int DoSomethingSpecific(int num, string str)
			{
				return num;
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
			int testNum = 0;

			TestClass1 tc1 = new TestClass1();
			TestClass2 tc2 = new TestClass2();

			WeakReference<TestClass2> tc2ref = new WeakReference<TestClass2>(tc2);
			WeakReference<TestClass1> tc1ref = new WeakReference<TestClass1>(tc1);

			var eventInfo1 = typeof(TestClass1).GetTypeInfo().GetDeclaredEvent("MyEvent");

			WeakEventSubscriber subscriber1 = new WeakEventSubscriber(tc2, (target, parameters) => testNum = ((TestClass2)target).DoSomethingSpecific((int)parameters[0], (string)parameters[1]));
			subscriber1.Subscribe(eventInfo1, tc1);

			var eventInfo2 = typeof(TestClass1).GetTypeInfo().GetDeclaredEvent("EightParameterEvent");
			
			WeakEventSubscriber subscriber2 = new WeakEventSubscriber(tc2, (target, parameters) => ((TestClass2)target).DoSomethingGeneral(parameters));
			subscriber2.Subscribe(eventInfo2, tc1);

			tc1.OnMyEvent(6, "six");

			Assert.Equal(expected: 6, actual: testNum);

			tc2 = null;

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.False(tc2ref.TryGetTarget(out tc2));
			tc1.OnMyEvent(7, "seven");

			Assert.Equal(expected: 6, actual: testNum);

			tc1 = null;

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.False(tc1ref.TryGetTarget(out tc1));
		}

		[Fact]
		public void ManySubscribed()
		{
			TestClass1 tc1 = new TestClass1();
			Mock<TestClass2> tc2 = new Mock<TestClass2>() { CallBase = true };

			int numCalls1 = 5;
			int numCalls2 = 10;
			int numCalls3 = 15;
			int numCalls4 = 20;

			var eventInfo1 = typeof(TestClass1).GetTypeInfo().GetDeclaredEvent("MyEvent");
			var eventInfo2 = typeof(TestClass1).GetTypeInfo().GetDeclaredEvent("FakePropertyChangedEvent");
			var eventInfo3 = typeof(TestClass1).GetTypeInfo().GetDeclaredEvent("EightParameterEvent");
			var eventInfo4 = typeof(TestClass1).GetTypeInfo().GetDeclaredEvent("NoParameterEvent");

			WeakEventSubscriber subscriber1 = new WeakEventSubscriber(tc2.Object, (target, parameters) => ((TestClass2)target).DoSomethingGeneral(parameters));
			subscriber1.Subscribe(eventInfo1, tc1);
			subscriber1.Subscribe(eventInfo2, tc1);
			subscriber1.Subscribe(eventInfo3, tc1);
			subscriber1.Subscribe(eventInfo4, tc1);

			Random rnd = new Random();

			while (numCalls1 + numCalls2 + numCalls3 + numCalls4 > 0)
			{
				switch (rnd.Next(1, 5))
				{
					case 1:
						if (numCalls1 > 0)
						{
							numCalls1--;
							tc1.OnMyEvent(1, "two");
						}
						break;
					case 2:
						if (numCalls2 > 0)
						{
							numCalls2--;
							tc1.OnFakePropertyChanged("property");
						}
						break;
					case 3:
						if (numCalls3 > 0)
						{
							numCalls3--;
							tc1.OnEightParameterEvent(1, 2, 3, 4, "five", true, new DateTime(7), 8.0);
						}
						break;
					case 4:
						if (numCalls4 > 0)
						{
							numCalls4--;
							tc1.OnNoParameterEvent();
						}
						break;
				}
			}

			tc2.Verify(tc => tc.DoSomethingGeneral(It.Is<Object[]>(param => param.Length == 2)), Times.Exactly(10 + 5));
			tc2.Verify(tc => tc.DoSomethingGeneral(It.Is<Object[]>(param => param.Length == 8)), Times.Exactly(15));
			tc2.Verify(tc => tc.DoSomethingGeneral(It.Is<Object[]>(param => param.Length == 0)), Times.Exactly(20));
		}
	}
}
