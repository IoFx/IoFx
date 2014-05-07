namespace System.IoFx.Connections
{
    public struct Context<T>
    {
        public T Unit { get; set; }
        public Connector<T> Parent { get; set; }

        public T Publish(T output)
        {
            Parent.Publish(output);
            return output;
        }
    }
}
