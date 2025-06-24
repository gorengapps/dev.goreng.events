using System;

namespace Framework.Events
{
    public class BaseEventProducer<T> : IDisposableEventProducer<T>
    {
        private readonly EventContainer<T> _container;
        private bool _disposed;
        
        public IStateRetainer<T> state => _container;
        public IEventListener<T> listener { get; }
        
        public BaseEventProducer(bool repeat = false)
        {
            _container = new EventContainer<T>();
            listener = new BaseEventListener<T>(_container, repeat);
        }

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