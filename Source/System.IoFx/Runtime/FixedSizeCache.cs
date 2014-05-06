using System.Collections.Concurrent;

namespace System.IoFx.Runtime
{
    class FixedSizeCache<T>
    {
        private readonly ConcurrentQueue<T> _cache;
        private readonly Func<T> _createFunc;
        private readonly int _maxCount;

        public FixedSizeCache(int maxCount, Func<T> create)
        {
            _maxCount = maxCount;
            _createFunc = create;
            _cache = new ConcurrentQueue<T>();
        }
        public IAwaiter<T> GetAwaiter()
        {
            return Take();
        }

        private IAwaiter<T> Take()
        {
            T item;
            if (_cache.TryDequeue(out item))
            {
                return new CacheAwaiter(item);
            }
            else
            {
                return new CacheAwaiter(_createFunc());
            }
        }

        public void Return(T item)
        {
            if (_cache.Count < _maxCount)
            {
                _cache.Enqueue(item);
            }
        }

        private struct CacheAwaiter : IAwaiter<T>
        {
            private readonly T _item;

            public CacheAwaiter(T item)
            {
                _item = item;
            }

            public T GetResult()
            {
                return _item;
            }

            public bool IsCompleted
            {
                get { return true; }
            }

            public void OnCompleted(Action continuation)
            {
                throw new NotImplementedException();
            }
        }
    }
}