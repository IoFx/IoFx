namespace System.IoFx.Connections
{
    public interface IConnection<out TResult, in TInputs> :IObservable<TResult>, IObserver<TInputs>
    {
    }


    public interface IConnection<TResult> : IConnection<TResult, TResult>
    {
    }
}
