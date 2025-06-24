using System;

namespace Framework.Events
{
    /// <summary>
    /// Internal implementation of an event listener that combines the latest values from two different event sources.
    /// Publishes combined tuples only after both sources have published at least one value.
    /// </summary>
    /// <typeparam name="T1">The type of data from the first event source.</typeparam>
    /// <typeparam name="T2">The type of data from the second event source.</typeparam>
    internal class CombineLatestEventListener<T1, T2> : IDisposableEventListener<(T1, T2)>
    {
        /// <summary>
        /// The internal event producer for publishing combined values.
        /// </summary>
        private readonly IDisposableEventProducer<(T1, T2)> _producer = new BaseEventProducer<(T1, T2)>();

        /// <summary>
        /// The latest value from the first source.
        /// </summary>
        private T1 _latest1;
        
        /// <summary>
        /// The latest value from the second source.
        /// </summary>
        private T2 _latest2;
        
        /// <summary>
        /// Indicates whether the first source has published at least one value.
        /// </summary>
        private bool _hasValue1;
        
        /// <summary>
        /// Indicates whether the second source has published at least one value.
        /// </summary>
        private bool _hasValue2;

        /// <summary>
        /// Subscription to the first event source.
        /// </summary>
        private readonly IDisposable _subscription1;
        
        /// <summary>
        /// Subscription to the second event source.
        /// </summary>
        private readonly IDisposable _subscription2;

        /// <summary>
        /// Initializes a new instance of the <see cref="CombineLatestEventListener{T1, T2}"/> class.
        /// </summary>
        /// <param name="source1">The first event source to combine.</param>
        /// <param name="source2">The second event source to combine.</param>
        public CombineLatestEventListener(IEventListener<T1> source1, IEventListener<T2> source2)
        {
            _subscription1 = source1.Subscribe((sender, data) =>
            {
                _latest1 = data;
                _hasValue1 = true;
                PublishCombined(sender);
            });

            _subscription2 = source2.Subscribe((sender, data) =>
            {
                _latest2 = data;
                _hasValue2 = true;
                PublishCombined(sender);
            });
        }

        /// <summary>
        /// Publishes the combined latest values if both sources have provided at least one value.
        /// </summary>
        /// <param name="sender">The sender of the triggering event.</param>
        private void PublishCombined(object sender)
        {
            if (_hasValue1 && _hasValue2)
            {
                _producer.Publish(sender, (_latest1, _latest2));
            }
        }

        /// <summary>
        /// Subscribes an event handler to receive combined values from both sources.
        /// </summary>
        /// <param name="handler">The event handler to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe the handler.</returns>
        public IDisposable Subscribe(EventHandler<(T1, T2)> handler)
        {
            return _producer.listener.Subscribe(handler);
        }

        /// <summary>
        /// Unsubscribes an event handler from receiving combined values.
        /// Explicit interface implementation.
        /// </summary>
        /// <param name="handler">The event handler to unsubscribe.</param>
        void IEventListener<(T1, T2)>.Unsubscribe(EventHandler<(T1, T2)> handler)
        {
            Unsubscribe(handler);
        }

        /// <summary>
        /// Unsubscribes an event handler from receiving combined values.
        /// </summary>
        /// <param name="handler">The event handler to unsubscribe.</param>
        public void Unsubscribe(EventHandler<(T1, T2)> handler)
        {
            _producer.listener.Unsubscribe(handler);
        }

        /// <summary>
        /// Disposes the combined event listener and unsubscribes from both source listeners.
        /// </summary>
        public void Dispose()
        {
            _producer.Dispose();
            _subscription1?.Dispose();
            _subscription2?.Dispose();
        }
    }
}