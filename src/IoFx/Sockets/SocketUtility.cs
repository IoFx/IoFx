using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace IoFx.Sockets
{
    class SocketUtility
    {
        public static EndPoint GetEndpoint(string hostnameOrAddress, int port)
        {
            return new DnsEndPoint(hostnameOrAddress,port);
        }

        internal static async Task<Socket> ConnectAsync(
            EndPoint endpoint,
            SocketType socketType = SocketType.Stream,
            ProtocolType protocolType = ProtocolType.Tcp)
        {
            var socket = new Socket(socketType, protocolType);
            bool disposeSocket = false;
            try
            {                
                using (SocketAwaitableEventArgs args = new SocketAwaitableEventArgs())
                {
                    args.RemoteEndPoint = endpoint;
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
                {
                    socket.Dispose();
                    socket = null;
                }
            }

            return socket;
        }
    }
}
