using System;

namespace Framework.Events
{
    public class EventContainer<T>: IEventContainer<T>, IDisposable
    { 
        public T lastState { get; private set; }
        private bool _disposed;
        
        private void SaveState(object sender, T data)
        {
            lastState = data;
        }

        public EventContainer()
        {
            publisher += SaveState;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                publisher -= SaveState;
                _disposed = true;
            }
        }
        
        public EventHandler<T> publisher { get; set; }
    }
}