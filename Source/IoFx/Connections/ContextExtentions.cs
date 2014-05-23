using System;
using System.Reactive.Linq;

namespace IoFx.Connections
{
    public static class ContextExtentions
    {
        public static IConnection<Context<T>, T> AsContexts<T>(this IConnection<T> inner)
        {
            return new ContextWrapper<T>(inner);
        }

        struct ContextWrapper<T> : IConnection<Context<T>, T>
        {
            private IConnection<T> _inner;

            public ContextWrapper(IConnection<T> inner)
            {
                _inner = inner;
            }

            public IDisposable Subscribe(IObserver<Context<T>> observer)
            {
                IConnection<T> channel = _inner;
                return _inner.Select(message =>
                {
                    return new Context<T>
                    {
                        Message = message,
                        Channel = channel
                    };
                }).Subscribe(observer);
            }


            public void Publish(T value)
            {
                _inner.Publish(value);
            }
        }
    }
}