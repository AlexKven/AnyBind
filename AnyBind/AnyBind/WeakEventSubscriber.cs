using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AnyBind
{
    internal abstract class WeakEventSubscriber : IDisposable
    {
        #region EventHandlerForwarder classes
        internal abstract class EventHandlerForwarder
        {
            public Action<object[]> Delegate { get; set; }
        }

        internal class EventHandlerForwarder<T> : EventHandlerForwarder
        {
            public void Handler(T param1)
            {
                Delegate(new object[] { param1 });
            }
        }

        internal class EventHandlerForwarder<T1, T2> : EventHandlerForwarder
        {
            public void Handler(T1 param1, T2 param2)
            {
                Delegate(new object[] { param1, param2 });
            }
        }

        internal class EventHandlerForwarder<T1, T2, T3> : EventHandlerForwarder
        {
            public void Handler(T1 param1, T2 param2, T3 param3)
            {
                Delegate(new object[] { param1, param2, param3 });
            }
        }

        internal class EventHandlerForwarder<T1, T2, T3, T4> : EventHandlerForwarder
        {
            public void Handler(T1 param1, T2 param2, T3 param3, T4 param4)
            {
                Delegate(new object[] { param1, param2, param3, param4 });
            }
        }

        internal class EventHandlerForwarder<T1, T2, T3, T4, T5> : EventHandlerForwarder
        {
            public void Handler(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
            {
                Delegate(new object[] { param1, param2, param3, param4, param5 });
            }
        }

        internal class EventHandlerForwarder<T1, T2, T3, T4, T5, T6> : EventHandlerForwarder
        {
            public void Handler(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
            {
                Delegate(new object[] { param1, param2, param3, param4, param5, param6 });
            }
        }

        internal class EventHandlerForwarder<T1, T2, T3, T4, T5, T6, T7> : EventHandlerForwarder
        {
            public void Handler(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7)
            {
                Delegate(new object[] { param1, param2, param3, param4, param5, param6, param7 });
            }
        }

        internal class EventHandlerForwarder<T1, T2, T3, T4, T5, T6, T7, T8> : EventHandlerForwarder
        {
            public void Handler(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8)
            {
                Delegate(new object[] { param1, param2, param3, param4, param5, param6, param7, param8 });
            }
        }
        #endregion

        private class HandleState
        {
            public EventInfo EventInfo { get; set; }
            public WeakReference TargetReference { get; set; }
            public Action<object, object[]> Handler { get; set; }
        }

        private WeakReference targetReference;

        private List<HandleState> handlers = new List<HandleState>();

        public WeakEventSubscriber(object target)
        {
            targetReference = new WeakReference(target);
            
        }
        
        public void Subscribe(EventInfo eventInfo, object declaringTarget, Action<object, object[]> handler)
        {
            EventHandlerForwarder forwarder;
            MethodInfo method = eventInfo.EventHandlerType.GetTypeInfo().GetDeclaredMethod("Invoke");

            var forwarderType = typeof(EventHandlerForwarder<,>).GetTypeInfo()
                .MakeGenericType(method.GetParameters().Select(pInfo => pInfo.GetType()).ToArray());

            forwarder = (EventHandlerForwarder)forwarderType.GetTypeInfo()
                .DeclaredConstructors.First().Invoke(new object[0]);

            eventInfo.AddEventHandler(declaringTarget, forwarder.Delegate);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
