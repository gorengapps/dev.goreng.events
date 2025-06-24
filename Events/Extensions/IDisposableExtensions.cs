using System;

namespace Framework.Events.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IDisposable"/> objects to simplify working with <see cref="DisposeBag"/>.
    /// </summary>
    public static class IDisposableExtensions {

        /// <summary>
        /// Adds this disposable object to the specified <see cref="DisposeBag"/>.
        /// </summary>
        /// <param name="disposable">The disposable object to add to the bag.</param>
        /// <param name="bag">The dispose bag to add the disposable to.</param>
        /// <remarks>
        /// This is a convenience method that allows for fluent syntax when adding disposables to a bag.
        /// </remarks>
        public static void AddToDisposables(this IDisposable disposable, DisposeBag bag)
        {
            bag.Add(disposable);   
        }
        
        
        /// <summary>
        /// Adds this disposable object to the specified <see cref="DisposeBag"/>.
        /// </summary>
        /// <param name="disposable">The disposable object to add to the bag.</param>
        /// <param name="bag">The dispose bag to add the disposable to.</param>
        /// <remarks>
        /// This is a convenience method that allows for fluent syntax when adding disposables to a bag.
        /// </remarks>
        public static T AddToDisposables<T>(this T disposable, DisposeBag bag) where T: IDisposable
        {
            bag.Add(disposable);
            return disposable;
        }
    }
}