using System;

namespace Framework.Events
{
    /// <summary>
    /// Represents a subscription to an event that can be disposed to automatically unsubscribe.
    /// Implements the disposable pattern to provide automatic cleanup of event subscriptions.
    /// </summary>
    /// <typeparam name="TEvent">The type of event data handled by this subscription.</typeparam>
    public class EventSubscription<TEvent> : IDisposable
    {
        /// <summary>
        /// The event listener that the subscription is associated with.
        /// </summary>
        private readonly IEventListener<TEvent> _listener;
        
        /// <summary>
        /// The event handler that was subscribed.
        /// </summary>
        private readonly EventHandler<TEvent> _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscription{TEvent}"/> class.
        /// </summary>
        /// <param name="listener">The event listener that manages the subscription.</param>
        /// <param name="handler">The event handler that was subscribed.</param>
        public EventSubscription(IEventListener<TEvent> listener, EventHandler<TEvent> handler)
        {
            _listener = listener;
            _handler = handler;
        }

        /// <summary>
        /// Disposes the subscription by unsubscribing the handler from the listener.
        /// </summary>
        public void Dispose()
        {
            _listener.Unsubscribe(_handler);
        }
    }
}