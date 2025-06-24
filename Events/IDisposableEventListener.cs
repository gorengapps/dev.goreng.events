using System;

namespace Framework.Events
{
    /// <summary>
    /// An interface that represents an event listener that is also disposable.
    /// </summary>
    public interface IDisposableEventListener<T> : IEventListener<T>, IDisposable {}
}