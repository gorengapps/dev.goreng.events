using System;

namespace Framework.Events
{
    internal class EventContainer<T>
    { 
        public T lastState { get; private set; }
        
        private void SaveState(object sender, T data)
        {
            lastState = data;
        }

        public EventContainer()
        {
            publisher += SaveState;
        }

        ~EventContainer()
        {
            publisher -= SaveState;
        }
        
        public EventHandler<T> publisher;
    }
}