namespace System.IoFx.Connections
{
    /// <summary>
    /// The IoChannel is a pipe through which type T would be produced and U would be consumed    
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    class Connection<TResult, TResponse> : IConnection<TResult,TResponse>
    {
        readonly IObservable<TResult> _producers;
        readonly IConsumer<TResponse> _consumer;        

        public Connection(IObservable<TResult> producer, IConsumer<TResponse> consumer)
        {
            this._producers = producer;
            this._consumer = consumer;
        }

        IDisposable IObservable<TResult>.Subscribe(IObserver<TResult> observer)
        {
            return this._producers.Subscribe(observer);
        }

        public void Publish(TResponse value)
        {
            this._consumer.Publish(value);
        }
    }

    class Connection<T> : Connection<T, T>, IConnection<T>
    {
        public Connection(IObservable<T> producer, IConsumer<T> consumer)
            : base(producer, consumer)
        {
        }
    }
}