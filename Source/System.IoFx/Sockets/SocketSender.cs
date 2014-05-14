using System.CodeDom;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading;

namespace System.IoFx.Sockets
{
    class SocketSender : IObserver<ArraySegment<byte>>, IDisposable, IVisitableConsumer<ArraySegment<byte>>
    {
        private int _disposed;
        private readonly Socket _socket;
        private bool _pending;
        private INotifyQueue<ArraySegment<byte>> _queueNotifier;

        public SocketSender(Socket socket)
        {
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
            TryConsume(value);
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
                if (shutdown && _socket.Connected)
                {
                    _socket.Shutdown(SocketShutdown.Send);
                }

                _socket.Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public bool TryConsume(ArraySegment<byte> item)
        {
            bool complete = false;
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
                    _queueNotifier.Enqueue(item);
                }

                return _pending;
            }
        }

        public void Accept(INotifyQueue<ArraySegment<byte>> visitor)
        {
            Contract.Assert(_queueNotifier == null);
            _queueNotifier.Visit(this);
            _queueNotifier = visitor;         
        }
    }

    internal interface IVisitorAcceptor<in T>
    {
        void Accept(T visitor);
    }


    interface IVisitor<in T>
    {
        void Visit(T visitor);
    }

    internal interface INotifyQueue<T> : IVisitor<IConsumer<T>>
    {
        void Enqueue(T item);
    }


    public interface IConsumer<in T> 
    {
        bool TryConsume(T item);        
    }

    internal interface IVisitableConsumer<T> : IConsumer<T>, IVisitorAcceptor<INotifyQueue<T>>
    {

    }
}