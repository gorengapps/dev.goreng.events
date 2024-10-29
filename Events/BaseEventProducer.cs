namespace Framework.Events
{
    public class BaseEventProducer<T> : IEventProducer<T>
    {
        private readonly EventContainer<T> _container;
        public IEventListener<T> listener { get; }
        
        public BaseEventProducer(bool repeat = false)
        {
            _container = new EventContainer<T>();
            listener = new BaseEventListener<T>(_container, repeat);
        }

        public void Publish(T data)
        {
            _container.publisher?.Invoke(this, data);
        }
    }
}