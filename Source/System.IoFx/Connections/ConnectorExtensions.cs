using System.Reactive.Linq;

namespace System.IoFx.Connections
{
    public static class ConnectorExtensions
    {
        public static IDisposable Consume<T>(this Connector<T> connection, IObservable<T> outputs)
        {
            return outputs.Subscribe(connection);
        }

        public static IDisposable Consume<TOutputs, TInputs>(this Composer<TOutputs, TInputs> composer, IObservable<TOutputs> outputs)
        {
            return outputs.Subscribe(composer.Outputs);
        }

        public static IObservable<Context<TOut>> ToConnection<TIn, TOut>(this IObservable<Context<TIn>> producer,
            Func<TIn, TOut> convertor)
        {
            var transformation = producer.Select(i => new Context<TOut>()
            {
                Data = convertor(i.Data)
            });

            return transformation;
        }

        public static IObservable<Context<TInput>> ToContexts<TInput>(this IObservable<TInput> elements)
        {
            return elements.Select(e => new Context<TInput>()
            {
                Data = e
            });
        }
    }
}