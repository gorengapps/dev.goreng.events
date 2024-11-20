namespace Framework.Events
{
    public class BaseEventProducer<T> : IEventProducer<T>
    {
        private readonly IEventContainer<T> _container;
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
    }
}