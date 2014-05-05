using System.Collections.Concurrent;
using System.IoFx.Runtime;
using System.Net.Sockets;
using System.Threading;

namespace System.IoFx.Sockets
{
    internal class SocketFactory
    {
        public static readonly SocketFactory Factory = new SocketFactory();
        private ConcurrentQueue<SocketAwaitable> _acceptAwaitableCache = new ConcurrentQueue<SocketAwaitable>();

        private readonly BufferManager _bufferManager;

        public SocketFactory()
            : this(new BufferManager())
        {
            _bufferManager = new BufferManager();
        }

        public SocketFactory(BufferManager bufferManager)
        {
            _bufferManager = bufferManager;
        }

        public SocketAwaitable GetSocketAwaitable()
        {           
            return new SocketAwaitable(new SocketAsyncEventArgs());
        }

        public BufferManager Buffer 
        {
            get { return _bufferManager; }
        }
    }

    public class SocketAwaitable : SocketAsyncEventArgs, IAwaiter
    {
        private readonly static Action Sentinel = () => { };

        private bool _wasCompleted;
        private Action _continuation;
        private readonly SocketAsyncEventArgs _eventArgs;

        internal SocketAwaitable(SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs == null)
                throw new ArgumentNullException("eventArgs");

            _wasCompleted = true;
            _eventArgs = eventArgs;
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

        public SocketAwaitable GetAwaiter() { return this; }

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
            if (_eventArgs.SocketError != SocketError.Success)
                throw new SocketException((int)_eventArgs.SocketError);
        }

        #endregion

    }
}
