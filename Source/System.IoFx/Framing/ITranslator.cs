namespace System.IoFx.Framing
{    
    internal interface ITranslator<in TInput, out TResult> : IObservable<TResult>
    {
        void OnNext(TInput item, IObserver<TResult> observer);
    }
}