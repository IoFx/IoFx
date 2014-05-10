namespace System.IoFx.Framing
{
    internal interface IEncoder<in TInput, out TResult> : IObservable<TResult>
    {
        void OnNext(TInput item, IObserver<TResult> observer);
    }
}