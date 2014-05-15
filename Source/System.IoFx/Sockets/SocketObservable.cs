using System.IoFx.Connections;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace System.IoFx.Sockets
{
    public static class SocketObservable
    {
        public static async Task<IDisposableConsumer<ArraySegment<byte>>> CreateTcpStreamSender(string hostname, int port)
        {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            bool disposeSocket = false;
            try
            {
                using (SocketAwaitableEventArgs args = new SocketAwaitableEventArgs())
                {
                    args.RemoteEndPoint = SocketHelpers.GetFirstIpEndPointFromHostName(hostname, port);
                    await socket.ConnectSocketAsync(args);
                }
            }
            catch (Exception)
            {
                disposeSocket = true;
                throw;
            }
            finally
            {
                if (disposeSocket)
                    socket.Dispose();
            }

            return socket.CreateSender();
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

        public static IObservable<IConnection<ArraySegment<byte>>> CreateTcpStreamListener(int port)
        {
            return GetTcpStreamSockets(port).GetConnections();
        }



        private static Socket StartTcpListenSocket(
            int port,
            IPAddress address = null,
            SocketType socketType = SocketType.Stream,
            ProtocolType protocolType = ProtocolType.Tcp,
            int backlog = 1024)
        {
            address = address ?? SocketHelpers.GetFirstIpEndPointFromHostName("localhost",port).Address;
            var socket = new Socket(socketType, protocolType);
            var endpoint = new IPEndPoint(address, port);
            socket.Bind(endpoint);
            socket.Listen(backlog);
            return socket;
        }
    }
}
