using System;

namespace Framework.Events
{
    public interface IEventProducer<T>
    {
        public IEventListener<T> listener { get; }
        public void Publish(object sender, T data);
    }
}