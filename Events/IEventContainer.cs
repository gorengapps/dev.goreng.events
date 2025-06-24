using System;

namespace Framework.Events
{
    /// <summary>
    /// Interface for an event container that combines state retention with event publishing capabilities.
    /// </summary>
    /// <typeparam name="T">The type of data handled by the event container.</typeparam>
    public interface IEventContainer<T>: IStateRetainer<T>
    {
        /// <summary>
        /// Gets or sets the event handler that publishes events of type T.
        /// </summary>
        /// <value>The event handler responsible for publishing events.</value>
        public EventHandler<T> publisher { get; set; }
    }
}