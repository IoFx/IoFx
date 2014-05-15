using System.Reactive.Linq;

namespace System.IoFx.Connections
{
    public static class ConnectionExtensions
    {
        public static IDisposable Consume<T>(this IConnection<T> connection, IObservable<T> outputs)
        {
            return outputs.Subscribe(output =>
            {
                connection.Publish(output);
            });
        }

        public static IDisposable Consume<TOutputs, TInputs>(this Composer<TOutputs, TInputs> composer, IObservable<TOutputs> outputs)
        {
            return outputs.Subscribe(composer.Outputs.Publish);
        }

        public static IObservable<Context<TOut>> ToConnection<TIn, TOut>(this IObservable<Context<TIn>> producer,
            Func<TIn, TOut> convertor)
        {
            var transformation = producer.Select(i => new Context<TOut>()
            {
                Message = convertor(i.Message)
            });

            return transformation;
        }

        public static IObservable<Context<TInput>> ToContexts<TInput>(this IObservable<TInput> elements)
        {
            return elements.Select(e => new Context<TInput>()
            {
                Message = e
            });
        }
    }
}