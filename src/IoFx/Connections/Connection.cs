using System;

namespace IoFx.Connections
{
    class Connection<TResult, TResponse> : IConnection<TResult,TResponse>
    {
        readonly IObservable<TResult> _producers;
        readonly IConsumer<TResponse> _consumer;

        public Connection(IProducer<TResult> producer, IConsumer<TResponse> consumer)
        {
            this._producers = producer;
            this._consumer = consumer;
        }

        public IDisposable Subscribe(IObserver<TResult> observer)
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
        public Connection(IProducer<T> producer, IConsumer<T> consumer)
            : base(producer, consumer)
        {
        }
    }
}