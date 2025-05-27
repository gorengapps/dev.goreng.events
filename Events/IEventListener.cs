using System;

namespace Framework.Events
{
    public interface IEventListener<T>
    {
        public IDisposable Subscribe(EventHandler<T> handler);
        public void Unsubscribe(EventHandler<T> handler);
    }
}