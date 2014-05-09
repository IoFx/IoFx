namespace System.IoFx.Connections
{
    public interface IConnector<out TResult, in TInputs> :IObservable<TResult>, IObserver<TInputs>
    {
    }


    public interface IConnector<TResult> : IConnector<TResult, TResult>
    {
    }
}
