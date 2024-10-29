using System;

namespace Framework.Events
{
    public class BaseEventListener<T> : IEventListener<T>
    {
        private readonly EventContainer<T> _container;
        private readonly bool _repeat;
        
        internal BaseEventListener(EventContainer<T> container, bool repeat)
        {
            _container = container;
            _repeat = repeat;
        }
        
        public event EventHandler<T> Subscribe
        {
            add
            {
                _container.publisher += value;

                if (_container.lastState != null && _repeat)
                {
                    value.Invoke(null, _container.lastState);
                }
            }
            remove => _container.publisher -= value;
        }
    }
}