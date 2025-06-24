using System;
using Framework.Events;

namespace Framework.Events.Extensions
{
    public static class EventListenerExtensions
    {
        /// <summary>
        /// Subscribe to <c>source</c> and publish every event on <c>target</c>.
        /// </summary>
        /// <param name="source">The event listener to subscribe to.</param>
        /// <param name="target">The event producer to publish events on.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe from the source.</returns>
        public static IDisposable PipeTo<T>(this IEventListener<T> source, IEventProducer<T> target)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));

            return source.Subscribe(target.Publish);
        }
        
        /// <summary>
        /// Subscribes to a source event, transforms the data using the provided function,
        /// and publishes the result on a target event producer.
        /// </summary>
        /// <typeparam name="T">The type of data from the source event.</typeparam>
        /// <typeparam name="Y">The type of data for the target event.</typeparam>
        /// <param name="source">The event source to listen to.</param>
        /// <param name="target">The event target to publish on.</param>
        /// <param name="transform">The function to convert data from type T to type Y.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe from the source.</returns>
        public static IDisposable PipeTo<T,Y>(this IEventListener<T> source, IEventProducer<Y> target, Func<T,Y> transform)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (transform == null) throw new ArgumentNullException(nameof(transform));
            
            return source.Subscribe((obj, data) => 
            {
                var transformedData = transform(data);
                target.Publish(obj, transformedData);
            });
        }
        
        /// <summary>
        /// Combines the latest values of two event listeners into a single event stream.
        /// The combined stream will only start publishing once both sources have published at least one value.
        /// </summary>
        /// <typeparam name="T1">The type of the first event source.</typeparam>
        /// <typeparam name="T2">The type of the second event source.</typeparam>
        /// <param name="source1">The first event listener.</param>
        /// <param name="source2">The second event listener.</param>
        /// <returns>A disposable event listener that publishes tuples of the latest values from both sources.</returns>
        public static IDisposableEventListener<(T1, T2)> CombineLatest<T1, T2>(this IEventListener<T1> source1, IEventListener<T2> source2)
        {
            return new CombineLatestEventListener<T1, T2>(source1, source2);
        }
    }
}