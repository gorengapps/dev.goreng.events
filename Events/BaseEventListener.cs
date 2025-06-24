using System;

namespace Framework.Events
{
    public class BaseEventListener<T> : IEventListener<T>
    {
        private readonly IEventContainer<T> _container;
        private readonly bool _repeat;

        public BaseEventListener(IEventContainer<T> container, bool repeat)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _repeat = repeat;
        }

        public IDisposable Subscribe(EventHandler<T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            
            _container.publisher += handler;
            
            if (_container.lastState != null && _repeat)
            {
                handler.Invoke(this, _container.lastState);
            }
            
            return new EventSubscription<T>(this, handler);
        }

        public void Unsubscribe(EventHandler<T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            
            _container.publisher -= handler;
        }
    }
}