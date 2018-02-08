using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
		}

		public class TestClass2
		{
			public void DoSomething(int num, string str)
			{
				
			}
		}

		[Fact]
		public void SimpleCase()
		{
			var tc1 = new TestClass1();
			var tc2 = new TestClass2();

			WeakEventSubscriber subscriber = new WeakEventSubscriber(tc2, (target, parameters) => ((TestClass2)target).DoSomething((int)parameters[0], (string)parameters[1]));
			subscriber.Subscribe(typeof(TestClass1).GetTypeInfo().GetDeclaredEvent("MyEvent"), tc1);

			tc1.OnMyEvent(5, "five");
		}
	}
}
