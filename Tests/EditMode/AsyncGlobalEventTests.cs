using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
using WezzelNL.GlobalEvents.Runtime;

namespace WezzelNL.GlobalEvents.Tests.EditMode
{
    public class AsyncGlobalEventTests
    {
        private struct AddAsyncListenerTestEvent : IAsyncGlobalEvent
        {
            public int Value;
        }

        private struct RemoveAsyncListenerTestEvent : IAsyncGlobalEvent
        {
            public int Value;
        }

        private struct MultipleAsyncListenersTestEvent : IAsyncGlobalEvent
        {
            public int Value;
        }

        private struct NoAsyncListenersTestEvent : IAsyncGlobalEvent
        {
            public int Value;
        }

        private struct CancellationTestEvent : IAsyncGlobalEvent
        {
            public int Value;
        }

        /// <summary>
        /// Verifies that <see cref="GlobalEvent.InvokeAsync"/> calls a registered async listener and passes the event data correctly.
        /// </summary>
        [UnityTest]
        public IEnumerator InvokeAsyncCallsRegisteredListener() =>
            UniTask.ToCoroutine(async () =>
            {
                var received = 0;
                async UniTask Handler(AddAsyncListenerTestEvent evt, CancellationToken ct)
                {
                    await UniTask.Yield();
                    received = evt.Value;
                }

                GlobalEvent.AddAsyncListener<AddAsyncListenerTestEvent>(Handler);
                await GlobalEvent.InvokeAsync(new AddAsyncListenerTestEvent { Value = 42 });

                Assert.AreEqual(42, received);
                GlobalEvent.RemoveAsyncListener<AddAsyncListenerTestEvent>(Handler);
            });

        /// <summary>
        /// Verifies that a listener removed via <see cref="GlobalEvent.RemoveAsyncListener"/> is no longer invoked when the event is raised.
        /// </summary>
        [UnityTest]
        public IEnumerator RemoveAsyncListenerStopsReceivingEvents() =>
            UniTask.ToCoroutine(async () =>
            {
                var callCount = 0;
                UniTask Handler(RemoveAsyncListenerTestEvent evt, CancellationToken ct)
                {
                    callCount++;
                    return UniTask.CompletedTask;
                }

                GlobalEvent.AddAsyncListener<RemoveAsyncListenerTestEvent>(Handler);
                GlobalEvent.RemoveAsyncListener<RemoveAsyncListenerTestEvent>(Handler);
                await GlobalEvent.InvokeAsync(new RemoveAsyncListenerTestEvent());

                Assert.AreEqual(0, callCount);
            });

        /// <summary>
        /// Verifies that <see cref="GlobalEvent.InvokeAsync"/> does not throw when no listeners are registered.
        /// </summary>
        [UnityTest]
        public IEnumerator InvokeAsyncWithNoListenersDoesNotThrow() =>
            UniTask.ToCoroutine(async () =>
            {
                await GlobalEvent.InvokeAsync(new NoAsyncListenersTestEvent());
            });

        /// <summary>
        /// Verifies that <see cref="GlobalEvent.InvokeAsync"/> awaits all registered listeners before completing.
        /// </summary>
        [UnityTest]
        public IEnumerator InvokeAsyncAwaitsAllListeners() =>
            UniTask.ToCoroutine(async () =>
            {
                var firstCompleted = false;
                var secondCompleted = false;

                async UniTask First(MultipleAsyncListenersTestEvent evt, CancellationToken ct)
                {
                    await UniTask.Yield();
                    await UniTask.Yield();
                    firstCompleted = true;
                }

                UniTask Second(MultipleAsyncListenersTestEvent evt, CancellationToken ct)
                {
                    secondCompleted = true;
                    return UniTask.CompletedTask;
                }

                GlobalEvent.AddAsyncListener<MultipleAsyncListenersTestEvent>(First);
                GlobalEvent.AddAsyncListener<MultipleAsyncListenersTestEvent>(Second);

                await GlobalEvent.InvokeAsync(new MultipleAsyncListenersTestEvent());

                Assert.IsTrue(firstCompleted);
                Assert.IsTrue(secondCompleted);

                GlobalEvent.RemoveAsyncListener<MultipleAsyncListenersTestEvent>(First);
                GlobalEvent.RemoveAsyncListener<MultipleAsyncListenersTestEvent>(Second);
            });

        /// <summary>
        /// Verifies that <see cref="GlobalEvent.InvokeAsync"/> runs all listeners concurrently rather than sequentially,
        /// so a slow listener does not block a fast listener from starting.
        /// </summary>
        [UnityTest]
        public IEnumerator InvokeAsyncRunsListenersConcurrentlyNotSequentially() =>
            UniTask.ToCoroutine(async () =>
            {
                var executionOrder = new System.Collections.Generic.List<string>();

                async UniTask Slow(MultipleAsyncListenersTestEvent evt, CancellationToken ct)
                {
                    executionOrder.Add("slow-start");
                    await UniTask.Yield();
                    await UniTask.Yield();
                    executionOrder.Add("slow-end");
                }

                UniTask Fast(MultipleAsyncListenersTestEvent evt, CancellationToken ct)
                {
                    executionOrder.Add("fast-start");
                    executionOrder.Add("fast-end");
                    return UniTask.CompletedTask;
                }

                GlobalEvent.AddAsyncListener<MultipleAsyncListenersTestEvent>(Slow);
                GlobalEvent.AddAsyncListener<MultipleAsyncListenersTestEvent>(Fast);

                await GlobalEvent.InvokeAsync(new MultipleAsyncListenersTestEvent());

                // If handlers ran sequentially, order would be slow-start, slow-end, fast-start, fast-end.
                // Concurrently, "fast" finishes entirely before "slow" resumes past its first yield.
                CollectionAssert.AreEqual(
                    new[] { "slow-start", "fast-start", "fast-end", "slow-end" },
                    executionOrder
                );

                GlobalEvent.RemoveAsyncListener<MultipleAsyncListenersTestEvent>(Slow);
                GlobalEvent.RemoveAsyncListener<MultipleAsyncListenersTestEvent>(Fast);
            });

        /// <summary>
        /// Verifies that the <see cref="CancellationToken"/> passed to <see cref="GlobalEvent.InvokeAsync"/> is forwarded to all registered listeners.
        /// </summary>
        [UnityTest]
        public IEnumerator InvokeAsyncPassesCancellationTokenToListener() =>
            UniTask.ToCoroutine(async () =>
            {
                using var cts = new CancellationTokenSource();
                var receivedToken = CancellationToken.None;

                UniTask Handler(CancellationTestEvent evt, CancellationToken ct)
                {
                    receivedToken = ct;
                    return UniTask.CompletedTask;
                }

                GlobalEvent.AddAsyncListener<CancellationTestEvent>(Handler);
                await GlobalEvent.InvokeAsync(new CancellationTestEvent(), cts.Token);

                Assert.AreEqual(cts.Token, receivedToken);
                GlobalEvent.RemoveAsyncListener<CancellationTestEvent>(Handler);
            });
    }
}
