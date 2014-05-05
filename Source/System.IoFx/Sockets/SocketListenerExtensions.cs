using System.IoFx.Connections;
using System.Net;
using System.Net.Sockets;

namespace System.IoFx.Sockets
{
    public static class SocketListenerExtensions
    {
        public static IListener<Socket> OnAccept(Func<Socket> createFunc)
        {
            return CreateListener(createFunc);
        }

        internal static SocketListener CreateListener(Func<Socket> createFunc)
        {
            return new SocketListener(createFunc, SocketFactory.Factory);
        }        
    }
}
