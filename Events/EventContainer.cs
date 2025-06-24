using System;

namespace Framework.Events
{
    /// <summary>
    /// A concrete implementation of <see cref="IEventContainer{T}"/> that manages event publishing and state retention.
    /// Automatically saves the state of each published event.
    /// </summary>
    /// <typeparam name="T">The type of data handled by the event container.</typeparam>
    public class EventContainer<T>: IEventContainer<T>, IDisposable
    { 
        /// <summary>
        /// Gets the last state that was published through this container.
        /// </summary>
        /// <value>The most recent data published, or the default value of T if nothing has been published.</value>
        public T lastState { get; private set; }
        private bool _disposed;
        
        /// <summary>
        /// Saves the state when an event is published.
        /// </summary>
        /// <param name="sender">The object that published the event.</param>
        /// <param name="data">The data that was published.</param>
        private void SaveState(object sender, T data)
        {
            lastState = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventContainer{T}"/> class.
        /// Automatically subscribes to save state when events are published.
        /// </summary>
        public EventContainer()
        {
            publisher += SaveState;
        }

        /// <summary>
        /// Disposes of the current subscription
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                publisher -= SaveState;
                _disposed = true;
            }
        }
        
        /// <summary>
        /// Gets or sets the event handler that publishes events of type T.
        /// </summary>
        /// <value>The event handler responsible for publishing events.</value>
        public EventHandler<T> publisher { get; set; }
    }
}