using System;

namespace IoFx.Connections
{
    public interface IDisposableConnection<T> : IConnection<T>, IDisposable
    {
 
    }
}