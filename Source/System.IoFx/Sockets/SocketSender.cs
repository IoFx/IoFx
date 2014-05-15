using System.CodeDom;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IoFx.Runtime;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading;

namespace System.IoFx.Sockets
{
    class SocketSender : IDisposableConsumer<ArraySegment<byte>>, IDisposable
    {
        private int _disposed;
        private readonly Socket _socket;
        private bool _pending;
        private IQueueable<ArraySegment<byte>> _queueVisitor;
        private SocketAsyncEventArgs _writeEventArgs;
        private IResourcePool<ArraySegment<byte>> _bufferManager = BufferManager.DefaultPool;
        private ArraySegment<byte> _writeBuffer;

        public SocketSender(Socket socket)
        {
            //TODO: Visit with buffer manager
            _writeEventArgs = new SocketAsyncEventArgs();                      
            _socket = socket;
        }

        public void OnCompleted()
        {
            Dispose(true);
        }

        public void OnError(Exception error)
        {
            Dispose(false);
        }

        public void OnNext(ArraySegment<byte> value)
        {
            Publish(value);
        }

        private int SendCore(ref ArraySegment<byte> value)
        {
            try
            {
                return _socket.Send(value.Array, value.Offset, value.Count, SocketFlags.None);
            }
            catch (Exception)
            {
                Dispose(false);
                throw;
            }
        }

        void Dispose(bool shutdown)
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                try
                {                    
                    _writeEventArgs.Dispose();

                    if (shutdown && _socket.Connected)
                    {
                        _socket.Shutdown(SocketShutdown.Send);
                    }

                    _socket.Close();

                }
                finally
                {
                    _bufferManager.Return(ref _writeBuffer);                    
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Publish(ArraySegment<byte> item)
        {            
            lock (_socket)
            {
                if (!_pending)
                {
                    var bytes = SendCore(ref item);
                    var remaining = item.Count - bytes;
                    _pending = remaining > 0;
                }
                else
                if (_pending)
                {
                    _queueVisitor.Enqueue(item);
                }              
            }
        }

        public void Accept(IQueueable<ArraySegment<byte>> visitor)
        {
            Contract.Assert(_queueVisitor == null);
            _queueVisitor.Visit(this);
            _queueVisitor = visitor;         
        }

        public void Accept(IResourcePool<ArraySegment<byte>> visitor)
        {
            _bufferManager = visitor;
            _writeEventArgs = new SocketAsyncEventArgs();

            var taken = visitor.Take(out _writeBuffer);
            Contract.Assert(taken);
            _writeEventArgs.SetBuffer(_writeBuffer.Array, _writeBuffer.Offset, _writeBuffer.Count);            
            
        }
    }
}