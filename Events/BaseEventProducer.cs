namespace Framework.Events
{
    public class BaseEventProducer<T> : IEventProducer<T>
    {
        private readonly EventContainer<T> _container;
        public IEventListener<T> listener { get; }
        
        public BaseEventProducer()
        {
            _container = new EventContainer<T>();
            listener = new BaseEventListener<T>(_container);
        }

        public void Publish(T data)
        {
            _container.publisher?.Invoke(this, data);
        }
    }
}