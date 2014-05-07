using System.Net.Sockets;

namespace System.IoFx.Sockets
{
    public static class SocketSenderExtensions
    {
        public static IObserver<ArraySegment<byte>> CreateSender(this Socket socket)
        {
            return new SocketSender(socket);
        }        
    }
}
