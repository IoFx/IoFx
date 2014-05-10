namespace System.IoFx.Connections
{
    public class DisposableConnection<T> : Connection<T>, IDisposable
    {
        private readonly IDisposable _disposable;

        public DisposableConnection(IObservable<T> producer,
                                    IObserver<T> consumer,
                                    IDisposable disposable)
            : base(producer, consumer)
        {
            _disposable = disposable;
        }

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
        }
    }
}