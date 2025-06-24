namespace Framework.Events
{
    /// <summary>
    /// Interface for objects that can retain and provide access to the last state of type T.
    /// </summary>
    /// <typeparam name="T">The type of state being retained.</typeparam>
    public interface IStateRetainer<out T>
    {
        /// <summary>
        /// Gets the last state that was retained.
        /// </summary>
        /// <value>The most recent state of type T, or null if no state has been set.</value>
        public T lastState { get; }
    }
}