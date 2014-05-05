using System.IoFx.Connections;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.IoFx.Sockets
{
    public static class SocketObservable
    {
        public static IListener<Socket> AcceptTcpStream(int port)
        {
            Func<Socket> createFunc = () => StartTcpListenSocket(port);
            return SocketListenerExtensions.OnAccept(createFunc);
        }

        public static IListener<IConnection<ArraySegment<byte>>> CreateTcpStreamListener(int port)
        {
            Func<Socket> createFunc = () => StartTcpListenSocket(port);
            var listener = SocketListenerExtensions.CreateListener(createFunc);
            var connections = listener.Select(s => new SocketConnection(s));
            return new StreamListener(listener, connections);
        }

        private static Socket StartTcpListenSocket(
            int port,
            IPAddress address = null,
            SocketType socketType = SocketType.Stream,
            ProtocolType protocolType = ProtocolType.Tcp,
            int backlog = 1024)
        {
            address = address ?? IPAddress.Loopback;
            var socket = new Socket(socketType, protocolType);
            var endpoint = new IPEndPoint(address, port);
            socket.Bind(endpoint);
            socket.Listen(backlog);
            return socket;
        }

        private class StreamListener : IListener<IConnection<ArraySegment<byte>>>
        {
            private readonly IListener<Socket> _listener;
            private readonly IObservable<IConnection<ArraySegment<byte>>> _connections;

            public StreamListener(IListener<Socket> listener,
                IObservable<IConnection<ArraySegment<byte>>> connections)
            {
                _listener = listener;
                _connections = connections;
            }

            public Task Start()
            {
                return _listener.Start();
            }

            public void Dispose()
            {
                _listener.Dispose();
            }

            public IDisposable Subscribe(IObserver<IConnection<ArraySegment<byte>>> observer)
            {
                return _connections.Subscribe(observer);
            }
        }

        class SocketConnection : IConnection<ArraySegment<byte>>
        {
            private readonly Socket _socket;
            private readonly IObservable<ArraySegment<byte>> _receiver;
            private readonly IObserver<ArraySegment<byte>> _sender;
            private int _disposed;

            public SocketConnection(Socket receiveSocket)
            {
                _socket = receiveSocket;
                _receiver = _socket.CreateReceiver();
                _sender = _socket.CreateSender();
            }

            public IObserver<ArraySegment<byte>> Sender
            {
                get { return _sender; }
            }

            public IDisposable Subscribe(IObserver<ArraySegment<byte>> observer)
            {
                return _receiver.Subscribe(observer);
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
        }
    }
}
