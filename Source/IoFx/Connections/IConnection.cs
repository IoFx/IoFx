using System;

namespace IoFx.Connections
{
    public interface IConnection<out TResult, in TInputs> :IProducer<TResult>, IConsumer<TInputs>
    {
    }

    public interface IConnection<TResult> : IConnection<TResult, TResult>
    {
    }

    public interface IDisposableConnection<T> : IConnection<T>, IDisposable
    {
 
    }
}
