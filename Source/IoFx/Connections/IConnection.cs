namespace IoFx.Connections
{
    /// <summary>
    /// The Connection produces TResult and TInputs can be published.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface IConnection<out TResult, in TResponse> : IProducer<TResult>, IConsumer<TResponse>
    {
    }

    public interface IConnection<TResult> : IConnection<TResult, TResult>
    {
    }
}
