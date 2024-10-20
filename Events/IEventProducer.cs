using System;

namespace Framework.Events
{
    public interface IEventProducer<T>
    {
        public IEventListener<T> listener { get; }
    }
}