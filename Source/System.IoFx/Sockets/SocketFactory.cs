using System.Collections.Concurrent;
using System.IoFx.Runtime;
using System.Net.Sockets;

namespace System.IoFx.Sockets
{
    internal class SocketFactory
    {
        public static readonly SocketFactory Factory = new SocketFactory();
        private ConcurrentQueue<SocketAwaitableEventArgs> _acceptAwaitableCache = new ConcurrentQueue<SocketAwaitableEventArgs>();

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

        public SocketAwaitableEventArgs GetSocketAwaitable()
        {           
            return new SocketAwaitableEventArgs();
        }

        public BufferManager Buffer 
        {
            get { return _bufferManager; }
        }
    }
}