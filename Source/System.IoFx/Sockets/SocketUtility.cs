using System;
using System.Collections.Generic;
using System.IoFx.Connections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace System.IoFx.Sockets
{
    public class SocketUtility
    {
        public static IPEndPoint GetFirstIpEndPoint(string hostName, int port)
        {
            var addresses = System.Net.Dns.GetHostAddresses(hostName);
            if (addresses.Length == 0)
            {
                throw new ArgumentException(
                    "Unable to retrieve address from specified host name.",
                    "hostName"
                );
            }

            return new IPEndPoint(addresses[0], port); // Port gets validated here.
        }

        public static async Task<IDisposableConnection<ArraySegment<byte>>> CreateConnection(
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

            return socket.ToConnection();
        }
    }
}
