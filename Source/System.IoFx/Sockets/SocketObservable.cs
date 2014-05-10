using System.IoFx.Connections;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;

namespace System.IoFx.Sockets
{
    public static class SocketObservable
    {
        public static IListener<Socket> AcceptTcpStream(int port)
        {
            Func<Socket> createFunc = () => StartTcpListenSocket(port);
            return new SocketListener(createFunc, SocketFactory.Factory);
        }

        public static IListener<IConnection<ArraySegment<byte>>> CreateTcpStreamListener(int port)
        {
            Func<Socket> createFunc = () => StartTcpListenSocket(port);
            var listener = new SocketListener(createFunc, SocketFactory.Factory);
            var connections = listener.Select(SocketConnection.Create);
            return new ConnectionAcceptor<Socket,ArraySegment<byte>> (listener, connections);
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
    }
}
