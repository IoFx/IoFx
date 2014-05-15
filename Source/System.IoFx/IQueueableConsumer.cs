namespace System.IoFx
{
    /// <summary>
    /// Synchronized Enqueuable dispatcher
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface IQueueableConsumer<T> : IConsumer<T>, IVisitorAcceptor<IQueueable<T>>
    {

    }
}