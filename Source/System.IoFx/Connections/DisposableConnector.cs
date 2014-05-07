namespace System.IoFx.Connections
{
    public class DisposableConnector<T> : Connector<T>, IDisposable
    {
        private readonly IDisposable _disposable;

        public DisposableConnector(IObservable<T> producer,
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