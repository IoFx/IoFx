namespace System.IoFx.Framing
{
    /// <summary>
    /// The Encoder provides a state machine for input parsing 
    /// and enables materializing results. The OnNext method is called when an input 
    /// arrives and the parser can continue processing and 
    /// notify the observer when a unit of TResult can be produced. 
    /// </summary>
    /// <typeparam name="TInput">Type of the Observable stream.</typeparam>
    /// <typeparam name="TResult">Type of subscription stream.</typeparam>
    struct TranslatorObservable<TInput, TResult> : IObservable<TResult>
    {
        private readonly IObservable<TInput> _inputs;
        private ITranslator<TInput, TResult> _translator;

        public TranslatorObservable(IObservable<TInput> inputs, ITranslator<TInput, TResult> translator)
        {
            _inputs = inputs;
            _translator = translator;
        }

        public IDisposable Subscribe(IObserver<TResult> observer)
        {
            var consumer = new ConsumerFromObserver<TResult>(observer);
            var translator = _translator;
            return _inputs.Subscribe(item =>
            {
                translator.OnNext(item, consumer);
            },
            observer.OnError,
            observer.OnCompleted);
        }

        /// <summary>
        /// Producers invoke the OnNext when an item is availabe for decoding
        /// </summary>
        /// <param name="item"></param>
        /// <param name="observer"></param>

        public class ConsumerFromObserver<T> : IConsumer<T>
        {
            private IObserver<T> _observer;
            public ConsumerFromObserver(IObserver<T> observer)
            {
                _observer = observer;
            }

            public void Publish(T value)
            {
                _observer.OnNext(value);
            }
        }
    }
}
