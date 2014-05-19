using System.Reactive.Linq;

namespace System.IoFx.Connections
{
    [Obsolete]
    public static class ConnectionExtensions
    {
        [Obsolete]
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

        public static IObservable<Context<TOut>> ToConnection<TIn, TOut>(this IProducer<Context<TIn>> producer,
            Func<TIn, TOut> convertor)
        {
            var transformation = producer.Select(i => new Context<TOut>()
            {
                Message = convertor(i.Message)
            });

            return transformation;
        }

        public static IObservable<Context<TInput>> ToContexts<TInput>(this IProducer<TInput> elements)
        {
            return elements.Select(e => new Context<TInput>()
            {
                Message = e
            });
        }


        //class ProducerTranslator<TInput,TOutput> : IProducer<TOutput>
        //{
        //    private Func<TInput, TOutput> _convertor;
        //    private IProducer<TInput> _input;
        //    public ProducerTranslator(IProducer<TInput> input, Func<TInput,TOutput> convertor)
        //    {
        //        _input = input;
        //        _convertor = convertor;
        //    }

        //    public IDisposable Subscribe(IObserver<TOutput> observer)
        //    {
        //        Observer.Create()
        //        return _input.Subscribe(o =>
        //        {

        //        });
        //    }
        //}

    }
}