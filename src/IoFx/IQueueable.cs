namespace IoFx
{
    internal interface IQueueable<T> : IVisitor<IConsumer<T>>
    {
        void Enqueue(T item);
    }
}