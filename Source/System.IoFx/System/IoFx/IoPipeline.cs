using System.Reactive.Linq;

namespace System.IoFx
{
    /// <summary>
    /// The IoChannel is a pipe through which type T would be produced and U would be consumed    
    /// </summary>
    /// <typeparam name="TInputs"></typeparam>
    /// <typeparam name="TOutputs"></typeparam>
    public class IoPipeline<TInputs, TOutputs> : IObservable<TInputs>, IObserver<TOutputs>
    {
        readonly IObservable<TInputs> _inputs;
        readonly IObserver<TOutputs> _outputs;

        public IoPipeline(IObservable<TInputs> producer, IObserver<TOutputs> consumer)
        {
            this._inputs = producer;
            this._outputs = consumer;
        }

        public IDisposable Subscribe(IObserver<TInputs> observer)
        {
            return this._inputs.Subscribe(observer);
        }

        public void OnCompleted()
        {
            this._outputs.OnCompleted();
        }

        public void OnError(Exception error)
        {
            this._outputs.OnError(error);
        }

        public void Publish(TOutputs value)
        {
            this._outputs.OnNext(value);
        }

        void IObserver<TOutputs>.OnNext(TOutputs value)
        {
            this._outputs.OnNext(value);
        }
    }

    public class IoPipeline<T> : IoPipeline<T, T>
    {
        public IoPipeline(IObservable<T> producer, IObserver<T> consumer)
            : base(producer, consumer)
        {
        }
    }

    public static class IoPipelineExtensions
    {
        public static IDisposable Consume<T>(this IoPipeline<T> pipeline, IObservable<T> outputs)
        {
            return outputs.Subscribe(pipeline);
        }

        public static IDisposable Consume<TOutputs, TInputs>(this Composer<TOutputs, TInputs> composer, IObservable<TOutputs> outputs)
        {
            return outputs.Subscribe(composer.Outputs);
        }

        public static IObservable<IoUnit<TOut>> ToPipeline<TIn, TOut>(this IObservable<IoUnit<TIn>> producer,
            Func<TIn, TOut> convertor)
        {
            var transformation = producer.Select(i => new IoUnit<TOut>()
            {
                Unit = convertor(i.Unit)
            });

            return transformation;
        }

        public static IObservable<IoUnit<TInput>> ToIoUnits<TInput>(this IObservable<TInput> elements)
        {
            return elements.Select(e => new IoUnit<TInput>()
            {
                Unit = e
            });
        }
    }
}