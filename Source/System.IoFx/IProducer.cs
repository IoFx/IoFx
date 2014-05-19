namespace System.IoFx
{
    public interface IProducer<out T> : IObservable<T>
    {
    }
}
