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
    public class Translator<TInput, TResult> : ITranslator<TInput, TResult>
    {
        private readonly IObservable<TInput> _inputs;

        public Translator(IObservable<TInput> inputs)
        {
            _inputs = inputs;
        }

        public IDisposable Subscribe(IObserver<TResult> observer)
        {
            return _inputs.Subscribe(item =>
            {
                this.OnNext(item, observer);
            },
            observer.OnError,
            observer.OnCompleted);
        }

        /// <summary>
        /// Producers invoke the OnNext when an item is availabe for decoding
        /// </summary>
        /// <param name="item"></param>
        /// <param name="observer"></param>
        public virtual void OnNext(TInput item, IObserver<TResult> observer)
        {
            observer.OnNext(default(TResult));
        }
    }
}
