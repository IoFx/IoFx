namespace System.IoFx.Connections
{
    /// <summary>
    /// The IoChannel is a pipe through which type T would be produced and U would be consumed    
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class Connector<TResult, TResponse> : IConnector<TResult,TResponse>
    {
        readonly IObservable<TResult> _producers;
        readonly IObserver<TResponse> _consumer;        

        public Connector(IObservable<TResult> producer, IObserver<TResponse> consumer)
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
            this._consumer.OnNext(value);
        }

        #region IObserver implementation
        void IObserver<TResponse>.OnCompleted()
        {
            this._consumer.OnCompleted();
        }

        void IObserver<TResponse>.OnError(Exception error)
        {
            this._consumer.OnError(error);
        }


        void IObserver<TResponse>.OnNext(TResponse value)
        {
            this._consumer.OnNext(value);
        } 
        #endregion
    }

    public class Connector<T> : Connector<T, T>, IConnector<T>
    {
        public Connector(IObservable<T> producer, IObserver<T> consumer)
            : base(producer, consumer)
        {
        }
    }
}