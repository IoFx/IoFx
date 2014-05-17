using System.IoFx.Sockets;

namespace System.IoFx.Connections
{
    public interface IConnection<out TResult, in TInputs> :IObservable<TResult>, IConsumer<TInputs>
    {
    }

    public interface IConnection<TResult> : IConnection<TResult, TResult>
    {
    }

    public interface IDisposableConnection<T> : IConnection<T>, IDisposable
    {
 
    }
}
