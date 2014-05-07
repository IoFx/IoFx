namespace System.IoFx.Connections
{
    public interface IConnector<out TResult, in TOutput> :IObservable<TResult>, IObserver<TOutput>
    {
    }


    public interface IConnector<TResult> : IConnector<TResult, TResult>
    {
    }
}
