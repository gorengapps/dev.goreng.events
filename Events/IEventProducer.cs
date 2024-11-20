using System;

namespace Framework.Events
{
    public interface IEventProducer<T>
    {
        public IStateRetainer<T> state { get; }
        public IEventListener<T> listener { get; }
        public void Publish(object sender, T data);
    }
}