using System;

namespace Framework.Events
{
    public interface IEventContainer<T>: IStateRetainer<T>
    {
        public EventHandler<T> publisher { get; set; }
    }
}