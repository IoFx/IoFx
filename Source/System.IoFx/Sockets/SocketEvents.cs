using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading.Tasks;
using IoFx.Connections;

namespace IoFx.Sockets
{
    public static class SocketEvents
    {
        public static async Task<IDisposableConnection<ArraySegment<byte>>> CreateConnection(string hostname, int port)
        {
            var socket =  await SocketUtility.ConnectAsync(new DnsEndPoint(hostname, port), 
                                                        SocketType.Stream, 
                                                        ProtocolType.Tcp);

            return socket.ToConnection();
        }

        public static IDisposableConsumer<ArraySegment<byte>> CreateSender(this Socket socket)
        {
            return new SocketSender(socket);
        }

        public static IListener<Socket> GetTcpStreamSockets(int port)
        {
            Func<Socket> createFunc = () => StartTcpListenSocket(port);
            return new SocketListener(createFunc, SocketFactory.Factory);
        }

        public static IObservable<IConnection<ArraySegment<byte>>> GetConnections(this IListener<Socket> listener)
        {
            var connections = listener.Select(SocketConnection.Create);
            return new ConnectionAcceptor<Socket, ArraySegment<byte>>(listener, connections);
        }

        public static IDisposableConnection<ArraySegment<byte>> ToConnection(this Socket socket)
        {
            return SocketConnection.Create(socket);
        }

        public static IObservable<IConnection<ArraySegment<byte>>> CreateTcpStreamListener(int port)
        {
            return GetTcpStreamSockets(port).GetConnections();
        }

        public static IProducer<ArraySegment<byte>> CreateReceiver(this Socket socket)
        {
            return new SocketReceiver(socket, SocketFactory.Factory);
        }

        private static Socket StartTcpListenSocket(
            int port,
            SocketType socketType = SocketType.Stream,
            ProtocolType protocolType = ProtocolType.Tcp,
            int backlog = 1024)
        {
            var socket = new Socket(socketType, protocolType);
            var endpoint = new IPEndPoint(IPAddress.IPv6Any, port);
            socket.Bind(endpoint);
            socket.Listen(backlog);
#if DEBUG
            Console.WriteLine(socket.LocalEndPoint);
#endif
            return socket;
        }
    }
}
