using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.IoFx.Runtime
{
    public class AwaitableThrottle
    {
        static readonly Awaiter CompletedAwaiter = new Awaiter(Task.FromResult(true).GetAwaiter());
        private readonly int _maxConcurrent;
        private readonly Queue<TaskCompletionSource<bool>> _awaiters;
        private int _count;
        
        public AwaitableThrottle(int maxConcurrent)
        {            
            _awaiters = new Queue<TaskCompletionSource<bool>>();
            _maxConcurrent = maxConcurrent;
        }

        private object Thislock
        {
            get { return _awaiters; }
        }
        
        public Awaiter GetAwaiter()
        {
            TaskCompletionSource<bool> awaiter = null;

            lock (Thislock)
            {
                if (_count < _maxConcurrent)
                {
                    _count++;
                    return CompletedAwaiter;
                }

                awaiter = new TaskCompletionSource<bool>();
                _awaiters.Enqueue(awaiter);
            }

            return new Awaiter(awaiter.Task.GetAwaiter());
        }

        public void Release()
        {
            TaskCompletionSource<bool> completion = null;

            lock (Thislock)
            {
                if (_count > 0)
                {
                    _count--;
                }

                if (_awaiters.Count > 0)
                {
                    completion = _awaiters.Dequeue();
                }
            }

            if (completion != null)
            {
                completion.SetResult(true);
            }
        }

        public struct Awaiter : ICriticalNotifyCompletion
        {
            private TaskAwaiter<bool> _taskAwaiter;

            public Awaiter(TaskAwaiter<bool> taskAwaiter)
            {
                this._taskAwaiter = taskAwaiter;
            }

            public bool IsCompleted
            {
                get { return _taskAwaiter.IsCompleted; }
            }

            public void OnCompleted(Action continuation)
            {
                _taskAwaiter.OnCompleted(continuation);
            }

            public void GetResult() { }

            public void UnsafeOnCompleted(Action continuation)
            {
                _taskAwaiter.UnsafeOnCompleted(continuation);
            }
        }
    }
}
