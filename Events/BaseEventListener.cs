using System;

namespace Framework.Events
{
    /// <summary>
    /// A base implementation of <see cref="IEventListener{T}"/> that provides event subscription and unsubscription capabilities.
    /// Supports optional replay of the last state to new subscribers.
    /// </summary>
    /// <typeparam name="T">The type of data handled by the event listener.</typeparam>
    public class BaseEventListener<T> : IEventListener<T>
    {
        /// <summary>
        /// The event container that manages the events and state.
        /// </summary>
        private readonly IEventContainer<T> _container;
        
        /// <summary>
        /// Indicates whether new subscribers should receive the last state immediately upon subscription.
        /// </summary>
        private readonly bool _repeat;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseEventListener{T}"/> class.
        /// </summary>
        /// <param name="container">The event container to listen to.</param>
        /// <param name="repeat">If true, new subscribers will immediately receive the last state when they subscribe.</param>
        public BaseEventListener(IEventContainer<T> container, bool repeat)
        {
            _container = container;
            _repeat = repeat;
        }

        /// <summary>
        /// Subscribes an event handler to receive events from the container.
        /// </summary>
        /// <param name="handler">The event handler to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe the handler.</returns>
        /// <remarks>
        /// If repeat is enabled and there is a last state, the handler will be invoked immediately with that state.
        /// </remarks>
        public IDisposable Subscribe(EventHandler<T> handler)
        {
            _container.publisher += handler;
            
            if (_container.lastState != null && _repeat)
            {
                handler.Invoke(this, _container.lastState);
            }
            
            return new EventSubscription<T>(this, handler);
        }

        /// <summary>
        /// Unsubscribes an event handler from receiving events.
        /// </summary>
        /// <param name="handler">The event handler to unsubscribe.</param>
        public void Unsubscribe(EventHandler<T> handler)
        {
            _container.publisher -= handler;
        }
    }
}