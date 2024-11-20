namespace Framework.Events
{
    public interface IStateRetainer<out T>
    {
        public T lastState { get; }
    }
}