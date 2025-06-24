using System;

namespace Framework.Events
{
    public class EventSubscription<TEvent> : IDisposable
    {
        private readonly IEventListener<TEvent> _listener;
        private readonly EventHandler<TEvent> _handler;

        public EventSubscription(IEventListener<TEvent> listener, EventHandler<TEvent> handler)
        {
            _listener = listener ?? throw new ArgumentNullException(nameof(listener));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public void Dispose()
        {
            _listener.Unsubscribe(_handler);
        }
    }
}