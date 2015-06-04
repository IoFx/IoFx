using System;

namespace IoFx
{
    public interface IConsumer<in T> 
    {
        void Publish(T value);        
    }

    public interface IDisposableConsumer<in T> : IConsumer<T>, IDisposable
    {        
    }
}