namespace System.IoFx.Framing
{    
    internal interface ITranslator<in TInput, TResult> : IProducer<TResult>
    {
        void OnNext(TInput item, IConsumer<TResult> observer);
    }
}