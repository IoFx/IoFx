using System;

namespace IoFx
{
    public interface IDisposableObservable<out TResult> : IObservable<TResult>, IDisposable
    {
    }
}
