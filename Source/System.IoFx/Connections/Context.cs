namespace System.IoFx.Connections
{
    public struct Context<T>
    {
        public T Data { get; set; }
        public Connector<T> Channel { get; set; }

        public T Publish(T output)
        {
            Channel.Publish(output);
            return output;
        }
    }
}
