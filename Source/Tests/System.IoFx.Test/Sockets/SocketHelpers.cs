using System.IoFx.Sockets;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace System.IoFx.Test.Sockets
{
    class SocketTestUtility
    {
        public static Socket Connect(int port)
        {
            var sender = new Socket(SocketType.Stream, ProtocolType.Tcp);
            var endpoint = SocketHelpers.GetFirstIpEndPointFromHostName("localhost", port);
            sender.Connect(endpoint);
            return sender;
        }

        public static int Send(Socket sender, string message)
        {
            byte[] msg = Encoding.ASCII.GetBytes(message);
            int bytesSent = sender.Send(msg);
            return bytesSent;
        }
    }
}
