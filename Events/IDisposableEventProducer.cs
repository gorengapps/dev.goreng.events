using System;

namespace Framework.Events
{
    public interface IDisposableEventProducer<T>: IEventProducer<T>, IDisposable
    {
        
    }
}