namespace System.IoFx
{
    public struct IoUnit<T>
    {
        public T Unit { get; set; }
        public IoPipeline<T> Parent { get; set; }

        public T Publish(T output)
        {
            Parent.Publish(output);
            return output;
        }
    }
}
