using System.Threading;
using Cysharp.Threading.Tasks;

namespace WezzelNL.GlobalEvents.Runtime
{
    /// <summary>
    /// The base interface marking that a specific struct is supposed to be used as a async global event
    /// </summary>
    public interface IAsyncGlobalEvent { }

    /// <summary>
    /// A container for all async handlers of a specific global event
    /// </summary>
    /// <typeparam name="TEvent">The type of the async global event of which the handlers are stored in this container</typeparam>
    public static class AsyncGlobalEventHandlers<TEvent> where TEvent : struct, IAsyncGlobalEvent
    {
        /// <summary>
        /// The delegate for async global event handlers
        /// </summary>
        public delegate UniTask AsyncEventHandler(TEvent evt, CancellationToken ct);

        /// <summary>
        /// The actual C# event that is raised to invoke the global event handlers
        /// </summary>
        public static event AsyncEventHandler OnEvent;

        /// <summary>
        /// Invokes an async global event. Calling this will call all listeners currently in this container.
        /// </summary>
        /// <remarks>Async listeners will execute in parallel. Order of execution is not guaranteed and listeners will not wait for the previous one to finish.</remarks>
        /// <param name="evt">The async global event to raise</param>
        /// <param name="ct">A cancellation token for all handlers</param>
        public static async UniTask InvokeAsync(TEvent evt, CancellationToken ct = default)
        {
            if (OnEvent == null) return;

            var invocationList = OnEvent.GetInvocationList();
            var tasks = new UniTask[invocationList.Length];
            for (var i = 0; i < invocationList.Length; i++)
            {
                var handler = (AsyncEventHandler)invocationList[i];
                tasks[i] = handler.Invoke(evt, ct);
            }

            await UniTask.WhenAll(tasks);
        }
    }
}
