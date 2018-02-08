using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AnyBind
{
    internal class WeakEventSubscriber : IDisposable
    {
        #region EventHandlerForwarder classes
        internal abstract class EventHandlerForwarder
        {
            public Action<object[]> Delegate { get; set; }

			public abstract Type DelegateType { get; }
        }

        internal class EventHandlerForwarder<T> : EventHandlerForwarder
        {
			public void Handler(T param1)
            {
                Delegate(new object[] { param1 });
            }

			public override Type DelegateType => typeof(Action<T>);
		}

        internal class EventHandlerForwarder<T1, T2> : EventHandlerForwarder
        {
            public void Handler(T1 param1, T2 param2)
            {
                Delegate(new object[] { param1, param2 });
			}

			public override Type DelegateType => typeof(Action<T1, T2>);
		}

        internal class EventHandlerForwarder<T1, T2, T3> : EventHandlerForwarder
        {
            public void Handler(T1 param1, T2 param2, T3 param3)
            {
                Delegate(new object[] { param1, param2, param3 });
			}

			public override Type DelegateType => typeof(Action<T1, T2, T3>);
		}

        internal class EventHandlerForwarder<T1, T2, T3, T4> : EventHandlerForwarder
        {
            public void Handler(T1 param1, T2 param2, T3 param3, T4 param4)
            {
                Delegate(new object[] { param1, param2, param3, param4 });
			}

			public override Type DelegateType => typeof(Action<T1, T2, T3, T4>);
		}

        internal class EventHandlerForwarder<T1, T2, T3, T4, T5> : EventHandlerForwarder
        {
            public void Handler(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
            {
                Delegate(new object[] { param1, param2, param3, param4, param5 });
			}

			public override Type DelegateType => typeof(Action<T1, T2, T3, T4, T5>);
		}

        internal class EventHandlerForwarder<T1, T2, T3, T4, T5, T6> : EventHandlerForwarder
        {
            public void Handler(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
            {
                Delegate(new object[] { param1, param2, param3, param4, param5, param6 });
			}

			public override Type DelegateType => typeof(Action<T1, T2, T3, T4, T5, T6>);
		}

        internal class EventHandlerForwarder<T1, T2, T3, T4, T5, T6, T7> : EventHandlerForwarder
        {
            public void Handler(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7)
            {
                Delegate(new object[] { param1, param2, param3, param4, param5, param6, param7 });
			}

			public override Type DelegateType => typeof(Action<T1, T2, T3, T4, T5, T6, T7>);
		}

        internal class EventHandlerForwarder<T1, T2, T3, T4, T5, T6, T7, T8> : EventHandlerForwarder
        {
            public void Handler(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8)
            {
                Delegate(new object[] { param1, param2, param3, param4, param5, param6, param7, param8 });
			}

			public override Type DelegateType => typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8>);
		}
        #endregion

        private class EventState
        {
            public EventInfo EventInfo { get; set; }
            public WeakReference DeclaringTargetReference { get; set; }
			public EventHandlerForwarder Forwarder { get; set; }

			public Delegate HandlerDelegate { get; set; }
        }

        private WeakReference HandleTargetReference { get; }
		private Action<object, object[]> Handler { get; }

		private List<EventState> Events = new List<EventState>();

        public WeakEventSubscriber(object handleTarget, Action<object, object[]> handler)
        {
            HandleTargetReference = new WeakReference(handleTarget);
			Handler = handler;
        }
        
        public void Subscribe(EventInfo eventInfo, object declaringTarget)
        {
            EventHandlerForwarder forwarder;
            MethodInfo method = eventInfo.EventHandlerType.GetTypeInfo().GetDeclaredMethod("Invoke");

            var forwarderType = typeof(EventHandlerForwarder<,>).GetTypeInfo()
                .MakeGenericType(method.GetParameters().Select(pInfo => pInfo.ParameterType).ToArray());

            forwarder = (EventHandlerForwarder)forwarderType.GetTypeInfo()
                .DeclaredConstructors.First().Invoke(new object[0]);

			forwarder.Delegate = ForwarderDelegate;

			var handlerMethodInfo = forwarderType.GetTypeInfo().GetDeclaredMethod("Handler");

			var handlerDelegate = handlerMethodInfo.CreateDelegate(forwarder.DelegateType, forwarder);


			eventInfo.AddEventHandler(declaringTarget, handlerDelegate);

			Events.Add(new EventState() { EventInfo = eventInfo, DeclaringTargetReference = new WeakReference(declaringTarget), Forwarder = forwarder, HandlerDelegate = handlerDelegate });
        }

		public void Unsubscribe(EventInfo eventInfo)
		{

		}

        public void Dispose()
        {
			foreach (var eventState in Events)
			{
				if (eventState.DeclaringTargetReference.TryGetTarget(out object target))
				{
					eventState.EventInfo.RemoveEventHandler(target, eventState.HandlerDelegate);
				}
			}
			Events.Clear();
        }

		private void ForwarderDelegate(object[] parameters)
		{
			object target;
			if (HandleTargetReference.IsAlive && (target = HandleTargetReference.Target) != null)
			{
				Handler(target, parameters);
			}
			else
			{
				Dispose();
			}
		}
    }
}
