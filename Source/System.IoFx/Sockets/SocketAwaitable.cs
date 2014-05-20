using System;
using System.Net.Sockets;
using System.Threading;
using IoFx.Runtime;

namespace IoFx.Sockets
{
    class SocketAwaitableEventArgs : SocketAsyncEventArgs, IAwaiter
    {
        private readonly static Action Sentinel = () => { };

        private bool _wasCompleted;
        private Action _continuation;        

        internal SocketAwaitableEventArgs()
        {
            _wasCompleted = true;            
        }

        protected override void OnCompleted(SocketAsyncEventArgs e)
        {
            base.OnCompleted(e);
            this.CompleteOperation();
            var prev = _continuation ?? Interlocked.CompareExchange(ref _continuation, Sentinel, null);
            if (prev != null)
            {
                prev();
            }
        }

        internal void StartOperation()
        {
            if (!_wasCompleted)
            {
                throw new InvalidOperationException("Cannot start operation on a pending awaiter.");
            }
            _wasCompleted = false;
            _continuation = null;
        }

        internal void CompleteOperation()
        {
            _wasCompleted = true;
        }

        #region Awaitable

        public SocketAwaitableEventArgs GetAwaiter() { return this; }

        public bool IsCompleted { get { return _wasCompleted; } }

        public void OnCompleted(Action continuation)
        {
            if (_continuation == Sentinel || Interlocked.CompareExchange(ref _continuation, continuation, null) == Sentinel)
            {
                // Run the continuation synchronously
                // TODO: Ensure no stack dives and if so do Task.Run()                
                continuation();
            }
        }

        public void GetResult()
        {
            if (this.SocketError != SocketError.Success)
                throw new SocketException((int)this.SocketError);
        }

        #endregion

    }
}
