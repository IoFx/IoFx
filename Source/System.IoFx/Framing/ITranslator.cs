namespace System.IoFx.Framing
{    
    /// <summary>
    /// The ITranslator defines the contract for the state machine
    /// the can produce 0,1..N items to the consumer. 
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    internal interface ITranslator<in TInput, TResult>
    {
        void OnNext(TInput item, IConsumer<TResult> observer);
    }
}