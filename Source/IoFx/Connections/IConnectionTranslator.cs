namespace IoFx.Connections
{
    public interface IConnectionTranslator<T1, T2> : IConnection<T2>
    {
        IConnection<T2> Translate(IConnection<T1> input);
    }
}
