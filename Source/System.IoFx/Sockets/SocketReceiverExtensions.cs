using System.Net.Sockets;

namespace System.IoFx.Sockets
{
    public static class SocketReceiverExtensions
    {
        public static IObservable<ArraySegment<byte>> GetData(this Socket socket)
        {
            return new SocketReceiver(socket, SocketFactory.Factory);
        }       
    }
}
