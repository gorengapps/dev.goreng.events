using System;

namespace Framework.Events
{
    public interface IEventListener<T>
    {
        event EventHandler<T> Subscribe;
    }
}