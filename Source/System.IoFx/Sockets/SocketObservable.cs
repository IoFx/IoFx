using System.IoFx.Connections;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace System.IoFx.Sockets
{
    public static class SocketObservable
    {
        public static IListener<Socket> AcceptTcpStream(int port)
        {
            Func<Socket> createFunc = () => StartTcpListenSocket(port);
            return new SocketListener(createFunc, SocketFactory.Factory);
        }

        public static IListener<IConnector<ArraySegment<byte>>> CreateTcpStreamListener(int port)
        {
            Func<Socket> createFunc = () => StartTcpListenSocket(port);
            var listener = new SocketListener(createFunc, SocketFactory.Factory);
            var connections = listener.Select(SocketConnection.Create);
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

        private class StreamListener : IListener<IConnector<ArraySegment<byte>>>
        {
            private readonly IListener<Socket> _listener;
            private readonly IObservable<IConnector<ArraySegment<byte>>> _connections;

            public StreamListener(IListener<Socket> listener, IObservable<IConnector<ArraySegment<byte>>> connections)
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

            public IDisposable Subscribe(IObserver<IConnector<ArraySegment<byte>>> observer)
            {
                return _connections.Subscribe(observer);
            }
        }

    
    }
}
