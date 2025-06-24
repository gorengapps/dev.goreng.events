using System;

namespace Framework.Events
{
    /// <summary>
    /// Interface for objects that can produce events of type T, providing both state retention and listener capabilities.
    /// </summary>
    /// <typeparam name="T">The type of data handled by the event producer.</typeparam>
    public interface IEventProducer<T>
    {
        /// <summary>
        /// Gets the state retainer that provides access to the last published state.
        /// </summary>
        /// <value>An <see cref="IStateRetainer{T}"/> for accessing the current state.</value>
        public IStateRetainer<T> state { get; }
        
        /// <summary>
        /// Gets the event listener that allows others to subscribe to events from this producer.
        /// </summary>
        /// <value>An <see cref="IEventListener{T}"/> for managing event subscriptions.</value>
        public IEventListener<T> listener { get; }
        
        /// <summary>
        /// Publishes an event with the specified data to all subscribers.
        /// </summary>
        /// <param name="sender">The object that is publishing the event.</param>
        /// <param name="data">The data to be published with the event.</param>
        public void Publish(object sender, T data);
    }
}