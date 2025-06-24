using System;

namespace Framework.Events
{
    /// <summary>
    /// Interface for objects that can listen to events of type T and manage event subscriptions.
    /// </summary>
    /// <typeparam name="T">The type of data handled by the event listener.</typeparam>
    public interface IEventListener<T>
    {
        /// <summary>
        /// Subscribes an event handler to receive events.
        /// </summary>
        /// <param name="handler">The event handler to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe the handler.</returns>
        public IDisposable Subscribe(EventHandler<T> handler);
        
        /// <summary>
        /// Unsubscribes an event handler from receiving events.
        /// </summary>
        /// <param name="handler">The event handler to unsubscribe.</param>
        public void Unsubscribe(EventHandler<T> handler);
    }
}