using System.Reactive.Linq;

namespace System.IoFx.Connections
{
    public struct Context<T>
    {
        public T Message { get; set; }
        public IConsumer<T> Channel { get; set; }

        public T Publish(T output)
        {
            Channel.Publish(output);
            return output;
        }
    }


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
