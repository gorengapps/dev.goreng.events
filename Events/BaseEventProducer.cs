using System;

namespace Framework.Events
{
    /// <summary>
    /// A base implementation of <see cref="IEventProducer{T}"/> that provides event production capabilities
    /// with integrated state retention and listener management.
    /// </summary>
    /// <typeparam name="T">The type of data produced by this event producer.</typeparam>
    public class BaseEventProducer<T> : : IDisposableEventProducer<T>
    {
        /// <summary>
        /// Signals if the producer was already disposed
        /// </summary>
        private bool _disposed;
    
        /// <summary>
        /// The internal event container that manages events and state.
        /// </summary>
        private readonly IEventContainer<T> _container;
        
        /// <summary>
        /// Gets the state retainer that provides access to the last published state.
        /// </summary>
        /// <value>An <see cref="IStateRetainer{T}"/> for accessing the current state.</value>
        public IStateRetainer<T> state => _container;
        
        /// <summary>
        /// Gets the event listener that allows others to subscribe to events from this producer.
        /// </summary>
        /// <value>An <see cref="IEventListener{T}"/> for managing event subscriptions.</value>
        public IEventListener<T> listener { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseEventProducer{T}"/> class.
        /// </summary>
        /// <param name="repeat">If true, new subscribers will immediately receive the last state when they subscribe.</param>
        public BaseEventProducer(bool repeat = false)
        {
            _container = new EventContainer<T>();
            listener = new BaseEventListener<T>(_container, repeat);
        }

        /// <summary>
        /// Publishes an event with the specified data to all subscribers.
        /// The data will also be saved as the new last state.
        /// </summary>
        /// <param name="sender">The object that is publishing the event.</param>
        /// <param name="data">The data to be published with the event.</param>
        public void Publish(object sender, T data)
        {
            _container.publisher?.Invoke(sender, data);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _container.Dispose();
                _disposed = true;
            }
        }
    }
}