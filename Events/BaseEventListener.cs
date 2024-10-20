using System;

namespace Framework.Events
{
    public class BaseEventListener<T> : IEventListener<T>
    {
        private readonly EventContainer<T> _container;
        
        internal BaseEventListener(EventContainer<T> container)
        {
            _container = container;
        }
        
        public event EventHandler<T> Subscribe
        {
            add
            {
                _container.publisher += value;

                if (_container.lastState != null)
                {
                    value.Invoke(null, _container.lastState);
                }
            }
            remove => _container.publisher -= value;
        }
    }
}