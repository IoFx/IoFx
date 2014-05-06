namespace System.IoFx.Connections
{
    public interface IConnection<TResult> :IObservable<TResult>, IDisposable
    {
        IObserver<TResult> Sender { get;}
    }
}
