using System;

namespace Framework.Events
{
    /// <summary>
    /// Interface for event producers that can be disposed to clean up resources and stop producing events.
    /// Combines event production capabilities with disposable resource management.
    /// </summary>
    /// <typeparam name="T">The type of data handled by the disposable event producer.</typeparam>
    public interface IDisposableEventProducer<T>: IEventProducer<T>, IDisposable
    {
        
    }
}