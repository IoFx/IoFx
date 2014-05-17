namespace System.IoFx.Connections
{   
    public struct Context<T>
    {
        public T Message { get; set; }
        public IConsumer<T> Channel { get; set; }

        public T Publish(T output)
        {
            Channel.Publish(output);
            return output;
        }
    }    
}
