using System;

namespace IoFx
{
    internal interface IResourcePool<T> : IVisitor<ArraySegment<byte>>
    {
        bool Take(out T resource);

        void Return(ref T resource);
    }
}