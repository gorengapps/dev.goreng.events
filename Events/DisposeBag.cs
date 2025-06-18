using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Framework.Events
{
    /// <summary>
    /// A container for managing multiple <see cref="IDisposable"/> instances, disposing them all when this bag is disposed.
    /// It also manages a <see cref="CancellationTokenSource"/> that is cancelled and disposed upon the bag's disposal.
    /// </summary>
    public class DisposeBag : IDisposable
    {
        /// <summary>
        /// List of disposables to be disposed when <see cref="Dispose"/> is called.
        /// </summary>
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        
        /// <summary>
        /// The cancellation token source that is managed by this bag.
        /// </summary>
        private readonly CancellationTokenSource _cancellationTokenSource;
        
        /// <summary>
        /// Indicates whether this <see cref="DisposeBag"/> has already been disposed.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Gets the <see cref="CancellationToken"/> associated with this bag.
        /// The token will be cancelled when the bag is disposed.
        /// </summary>
        public CancellationTokenSource token => _cancellationTokenSource;

        public DisposeBag()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            // Add the source to the list of disposables so it's disposed with the bag.
            _disposables.Add(_cancellationTokenSource);
        }
        
        /// <summary>
        /// Adds an <see cref="IDisposable"/> instance to the bag.
        /// </summary>
        /// <param name="disposable">The disposable instance to add.</param>
        /// <remarks>
        /// Disposables added after the bag has been disposed will still be disposed immediately.
        /// </remarks>
        public void Add(IDisposable disposable)
        {
            if (_isDisposed)
            {
                // If already disposed, dispose the item immediately to maintain consistency.
                disposable.Dispose();
                return;
            }

            _disposables.Add(disposable);
        }
        
        /// <summary>
        /// Cancels the token and disposes all <see cref="IDisposable"/> instances in the bag.
        /// </summary>
        /// <remarks>
        /// Calling Dispose multiple times has no additional effect.
        /// </remarks>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            // First, cancel the token to signal any async operations to stop.
            _cancellationTokenSource.Cancel();
            
            // Now, dispose all items, including the CancellationTokenSource itself.
            foreach (var disposable in _disposables)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    // Optionally log or handle exceptions from disposal of individual items
                    Debug.LogError($"Error disposing object of type {disposable.GetType()}: {ex}");
                }
            }

            _disposables.Clear();
        }
    }
}