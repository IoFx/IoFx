using System;
using System.Diagnostics.Contracts;

namespace IoFx.Runtime
{
    class BufferManager
    {
        private readonly BufferPool _buffers = new BufferPool(8 * 1024, 1024);

        public FixedSizeCache<byte[]> GetBufferAsync()
        {
            return _buffers.GetAwaiter();
        }

        public void Return(byte[] buffer)
        {
            _buffers.Return(buffer);
        }

        private class BufferPool
        {
            private readonly int _size;
            private readonly FixedSizeCache<byte[]> _cache;

            public BufferPool(int size, int maxCount)
            {
                _size = size;
                _cache = new FixedSizeCache<byte[]>(maxCount, () => new byte[_size]);
            }

            internal FixedSizeCache<byte[]> GetAwaiter()
            {
                return _cache;
            }

            internal void Return(byte[] buffer)
            {
                Contract.Assert(buffer.Length == _size);
                _cache.Return(buffer);
            }
        }


        public static IResourcePool<ArraySegment<byte>> DefaultPool = new DefaultAllocatorPool<ArraySegment<byte>>(() => new ArraySegment<byte>(new byte[8 * 1024]));
    }

    public class DefaultAllocatorPool<T> : IResourcePool<T>
    {
        private readonly Func<T> _createFunc;
        public DefaultAllocatorPool(Func<T> createFunc)
        {
            if (createFunc == null)
            {
                throw new ArgumentNullException("createFunc");
            }

            _createFunc = createFunc;
        }

        public bool Take(out T resource)
        {
            resource = _createFunc();
            return true;
        }

        public void Return(ref T resource)
        {
            // NOP
        }

        public void Visit(ArraySegment<byte> visitor)
        {
            throw new NotImplementedException();
        }
    }
}
