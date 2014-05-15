namespace System.IoFx.Connections
{
    /// <summary>
    /// This class avoid using Disposable.Create for connections. 
    /// The other option would be to make connection itself disposable. 
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class DisposableConnection<TRequest, TResponse> : Connection<TRequest, TResponse>, IDisposable
    {
        private readonly IDisposable _disposable;

        public DisposableConnection(IObservable<TRequest> producer,
                                    IConsumer<TResponse> consumer,
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


    public class DisposableConnection<T> : DisposableConnection<T, T> , IConnection<T>
    {

        public DisposableConnection(IObservable<T> producer,
                                    IConsumer<T> consumer,
                                    IDisposable disposable)
            : base(producer, consumer, disposable)
        {
        }
    }
}