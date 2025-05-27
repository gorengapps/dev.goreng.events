using System;

namespace Framework.Events.Extensions
{
    public static class IDisposableExtensions {

        public static void AddToDisposables(this IDisposable disposable, DisposeBag bag)
        {
            bag.Add(disposable);   
        }
    }
}