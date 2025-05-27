using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Events
{
    /// <summary>
    /// A container for managing multiple <see cref="IDisposable"/> instances, disposing them all when this bag is disposed.
    /// </summary>
    public class DisposeBag : IDisposable
    {
        /// <summary>
        /// List of disposables to be disposed when <see cref="Dispose"/> is called.
        /// </summary>
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        
        /// <summary>
        /// Indicates whether this <see cref="DisposeBag"/> has already been disposed.
        /// </summary>
        private bool _isDisposed;
        
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
        /// Disposes all <see cref="IDisposable"/> instances in the bag.
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