using NUnit.Framework;
using UnityEngine.TestTools.Constraints;
using WezzelNL.GlobalEvents.Runtime;

namespace WezzelNL.GlobalEvents.Tests.EditMode
{
    public class GlobalEventTests
    {
        private struct AddListenerTestEvent : IGlobalEvent
        {
            public int Value;
        }

        private struct RemoveListenerTestEvent : IGlobalEvent
        {
            public int Value;
        }

        private struct MultipleListenersTestEvent : IGlobalEvent
        {
            public int Value;
        }

        private struct NoListenersTestEvent : IGlobalEvent
        {
            public int Value;
        }

        /// <summary>
        /// Verifies that <see cref="GlobalEvent.Invoke"/> calls a registered listener and passes the event data correctly.
        /// </summary>
        [Test]
        public void InvokeCallsRegisteredListener()
        {
            var received = 0;
            void Handler(in AddListenerTestEvent evt) => received = evt.Value;

            GlobalEvent.AddListener<AddListenerTestEvent>(Handler);
            GlobalEvent.Invoke(new AddListenerTestEvent { Value = 42 });

            Assert.AreEqual(42, received);
        }

        /// <summary>
        /// Verifies that a listener removed via <see cref="GlobalEvent.RemoveListener"/> is no longer invoked when the event is raised.
        /// </summary>
        [Test]
        public void RemoveListenerStopsReceivingEvents()
        {
            var callCount = 0;
            void Handler(in RemoveListenerTestEvent evt) => callCount++;

            GlobalEvent.AddListener<RemoveListenerTestEvent>(Handler);
            GlobalEvent.RemoveListener<RemoveListenerTestEvent>(Handler);
            GlobalEvent.Invoke(new RemoveListenerTestEvent());

            Assert.AreEqual(0, callCount);
        }

        /// <summary>
        /// Verifies that <see cref="GlobalEvent.Invoke"/> calls all registered listeners for the same event type.
        /// </summary>
        [Test]
        public void InvokeCallsAllRegisteredListeners()
        {
            var firstCalled = false;
            var secondCalled = false;
            void First(in MultipleListenersTestEvent evt) => firstCalled = true;
            void Second(in MultipleListenersTestEvent evt) => secondCalled = true;

            GlobalEvent.AddListener<MultipleListenersTestEvent>(First);
            GlobalEvent.AddListener<MultipleListenersTestEvent>(Second);
            GlobalEvent.Invoke(new MultipleListenersTestEvent());

            Assert.IsTrue(firstCalled);
            Assert.IsTrue(secondCalled);
        }

        /// <summary>
        /// Verifies that <see cref="GlobalEvent.Invoke"/> does not throw when no listeners are registered.
        /// </summary>
        [Test]
        public void InvokeWithNoListenersDoesNotThrow()
        {
            Assert.DoesNotThrow(() => GlobalEvent.Invoke(new NoListenersTestEvent()));
        }

        private struct AllocTestEvent : IGlobalEvent
        {
            public int Value;
        }

        private static readonly GlobalEventHandlers<AllocTestEvent>.EventHandler handler = OnAllocTestEvent;

        private static void OnAllocTestEvent(in AllocTestEvent evt) { }

        [SetUp]
        public void SetUp() => GlobalEvent.RemoveListener(handler);

        /// <summary>
        /// Verifies that <see cref="GlobalEvent.AddListener"/> does not allocate GC memory.
        /// </summary>
        [Test]
        public void AddListenerDoesNotAllocate()
        {
            Assert.That(() => GlobalEvent.AddListener(handler),
                NUnit.Framework.Is.Not.AllocatingGCMemory());
            GlobalEvent.RemoveListener(handler);
        }

        /// <summary>
        /// Verifies that <see cref="GlobalEvent.RemoveListener"/> does not allocate GC memory.
        /// </summary>
        [Test]
        public void RemoveListenerDoesNotAllocate()
        {
            GlobalEvent.AddListener(handler);
            Assert.That(() => GlobalEvent.RemoveListener(handler),
                NUnit.Framework.Is.Not.AllocatingGCMemory());
        }

        /// <summary>
        /// Verifies that <see cref="GlobalEvent.Invoke"/> does not allocate GC memory when no listeners are registered.
        /// </summary>
        [Test]
        public void InvokeWithNoListenersDoesNotAllocate()
        {
            Assert.That(() => GlobalEvent.Invoke(new AllocTestEvent { Value = 1 }),
                NUnit.Framework.Is.Not.AllocatingGCMemory());
        }

        /// <summary>
        /// Verifies that <see cref="GlobalEvent.Invoke"/> does not allocate GC memory when a listener is registered.
        /// </summary>
        [Test]
        public void InvokeWithListenerDoesNotAllocate()
        {
            GlobalEvent.AddListener(handler);
            Assert.That(() => GlobalEvent.Invoke(new AllocTestEvent { Value = 1 }),
                NUnit.Framework.Is.Not.AllocatingGCMemory());
            GlobalEvent.RemoveListener(handler);
        }
    }
}
