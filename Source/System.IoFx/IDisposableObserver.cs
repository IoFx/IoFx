namespace System.IoFx
{
    public interface IDisposableObserver<out TResult>:IObservable<TResult>, IDisposable
    {
    }
}
