namespace System.IoFx
{
    public interface IDisposableObservable<out TResult> : IObservable<TResult>, IDisposable
    {
    }
}
