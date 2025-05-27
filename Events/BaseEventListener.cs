using System;

namespace Framework.Events
{
    public class BaseEventListener<T> : IEventListener<T>
    {
        private readonly IEventContainer<T> _container;
        private readonly bool _repeat;

        public BaseEventListener(IEventContainer<T> container, bool repeat)
        {
            _container = container;
            _repeat = repeat;
        }

        public IDisposable Subscribe(EventHandler<T> handler)
        {
            _container.publisher += handler;
            
            if (_container.lastState != null && _repeat)
            {
                _container.publisher.Invoke(this, _container.lastState);
            }
            
            return new EventSubscription<T>(this, handler);
        }

        public void Unsubscribe(EventHandler<T> handler)
        {
            _container.publisher -= handler;
        }
    }
}