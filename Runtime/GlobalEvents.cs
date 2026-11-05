using System.Threading;
using Cysharp.Threading.Tasks;

namespace WezzelNL.GlobalEvents.Runtime
{
    /// <summary>
    /// The base interface marking that a specific struct is supposed to be used as a global event
    /// </summary>
    public interface IGlobalEvent { }

    /// <summary>
    /// A container for all handlers of a specific global event
    /// </summary>
    /// <typeparam name="TEvent">The type of the global event of which the handlers are stored in this container</typeparam>
    public static class GlobalEventHandlers<TEvent> where TEvent : struct, IGlobalEvent
    {
        /// <summary>
        /// The delegate for global event handlers
        /// </summary>
        public delegate void EventHandler(in TEvent evt);

        /// <summary>
        /// The actual C# event that is raised to invoke the global event handlers
        /// </summary>
        public static event EventHandler OnEvent;

        /// <summary>
        /// Invokes a global event. Calling this will call all listeners currently in this container.
        /// </summary>
        /// <param name="evt">The global event to raise</param>
        public static void Invoke(in TEvent evt) => OnEvent?.Invoke(evt);
    }

    /// <summary>
    /// The API for global events
    /// </summary>
    public static class GlobalEvent
    {
        /// <summary>
        /// Adds a listener for this global event from the <see cref="GlobalEventHandlers{TEvent}"/> container for this event.
        /// </summary>
        /// <param name="handler">The handler to add as a listener</param>
        /// <typeparam name="TEvent">The type of global event to listen for</typeparam>
        public static void AddListener<TEvent>(in GlobalEventHandlers<TEvent>.EventHandler handler) where TEvent : struct, IGlobalEvent => 
	        GlobalEventHandlers<TEvent>.OnEvent += handler;

        /// <summary>
        /// Removes a listener for this global event from the <see cref="GlobalEventHandlers{TEvent}"/> container for this event.
        /// </summary>
        /// <param name="handler">The handler to remove as a listener</param>
        /// <typeparam name="TEvent">The type of global event to remove a listener for</typeparam>
        public static void RemoveListener<TEvent>(in GlobalEventHandlers<TEvent>.EventHandler handler) where TEvent : struct, IGlobalEvent => 
	        GlobalEventHandlers<TEvent>.OnEvent -= handler;

        /// <summary>
        /// Invokes a global event. Calling this will call all listeners currently in the <see cref="GlobalEventHandlers{TEvent}"/> container for this event.
        /// </summary>
        /// <param name="evt">The event to raise</param>
        /// <typeparam name="TEvent">The type of event raised</typeparam>
        public static void Invoke<TEvent>(in TEvent evt) where TEvent : struct, IGlobalEvent => 
	        GlobalEventHandlers<TEvent>.Invoke(evt);

        /// <summary>
        /// Adds an async listener for this global event from the <see cref="AsyncGlobalEventHandlers{TEvent}"/> container for this event.
        /// </summary>
        /// <param name="handler">The async handler to add as a listener</param>
        /// <typeparam name="TEvent">The type of async global event to listen for</typeparam>
        public static void AddAsyncListener<TEvent>(in AsyncGlobalEventHandlers<TEvent>.AsyncEventHandler handler) where TEvent : struct, IAsyncGlobalEvent => 
	        AsyncGlobalEventHandlers<TEvent>.OnEvent += handler;

        /// <summary>
        /// Removes an async listener for this global event from the <see cref="AsyncGlobalEventHandlers{TEvent}"/> container for this event.
        /// </summary>
        /// <param name="handler">The async handler to remove as a listener</param>
        /// <typeparam name="TEvent">The type of async global event to listen for</typeparam>
        public static void RemoveAsyncListener<TEvent>(in AsyncGlobalEventHandlers<TEvent>.AsyncEventHandler handler) where TEvent : struct, IAsyncGlobalEvent => 
	        AsyncGlobalEventHandlers<TEvent>.OnEvent -= handler;

        /// <summary>
        /// Invokes an async global event. Calling this will call all listeners currently in the <see cref="AsyncGlobalEventHandlers{TEvent}"/> container for this event.
        /// </summary>
        /// <param name="evt">The async global event to raise</param>
        /// <param name="ct">A cancellation token for the async handlers</param>
        /// <typeparam name="TEvent">The type of async global event to raise</typeparam>
        public static async UniTask InvokeAsync<TEvent>(TEvent evt, CancellationToken ct = default) where TEvent : struct, IAsyncGlobalEvent =>
            await AsyncGlobalEventHandlers<TEvent>.InvokeAsync(evt, ct);
    }
}
