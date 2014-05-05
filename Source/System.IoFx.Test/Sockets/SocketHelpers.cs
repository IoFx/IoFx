using System.Net;
using System.Net.Sockets;
using System.Text;

namespace System.IoFx.Test.Sockets
{
    class SocketHelpers
    {
        public static Socket Connect(int port)
        {
            var sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(new IPEndPoint(IPAddress.Loopback, port));
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
