using System;

namespace IoFx
{
    public interface IProducer<out T> : IObservable<T>
    {
    }
}
