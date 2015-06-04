namespace IoFx.Framing
{    
    /// <summary>
    /// The ITranslator defines the contract for the state machine
    /// the can produce 0,1..N items to the consumer. 
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface ITranslator<in TInput, out TResult>
    {
        void OnNext(TInput item, IConsumer<TResult> consumer);
    }
}