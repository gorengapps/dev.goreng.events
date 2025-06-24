using System;

namespace Framework.Events
{
    public class EventContainer<T>: IEventContainer<T>
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
        
        public EventHandler<T> publisher { get; set; }
    }
}