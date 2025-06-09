using System;
using Framework.Events;

namespace Extensions
{
    public static class EventListenerExtensions
    {
        /// <summary>
        /// Subscribe to <c>source</c> and publish every event on <c>target</c>.
        /// </summary>
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
        /// <param name="source">The event source to listen to.</param>
        /// <param name="target">The event target to publish on.</param>
        /// <param name="transform">The function to convert data from type T to type Y.</param>
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
    }
}