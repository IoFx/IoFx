using System.Net.Sockets;
using System.Threading.Tasks;

namespace System.IoFx.Sockets
{

    public static class SocketOperationsExtensions
    {
        private delegate bool SocketOperationDelegate(Socket socket, SocketAsyncEventArgs args);

        private static readonly SocketOperationDelegate AcceptAsyncHandler = (s, e) => s.AcceptAsync(e);
        private static readonly SocketOperationDelegate ConnectAsyncHandler = (s, e) => s.ConnectAsync(e);
        private static readonly SocketOperationDelegate ReceiveAsyncHandler = (s, e) => s.ReceiveAsync(e);
        private static readonly SocketOperationDelegate SendAsyncHandler = (s, e) => s.SendAsync(e);

        //TODO: Fix alloction per socket accept and remo task.
        public static async Task<Socket> AcceptSocketAsync(this Socket listenSocket, SocketAwaitable awaitable)
        {
            await AcceptAsync(listenSocket, awaitable);
            var acceptSocket = awaitable.AcceptSocket;
            awaitable.AcceptSocket = null;
            return acceptSocket;
        }

        public static SocketAwaitable AcceptAsync(this Socket socket, SocketAwaitable awaitable)
        {
            return OperationAsync(socket, awaitable, AcceptAsyncHandler);
        }
        
        public static SocketAwaitable ConnectSocketAsync(this Socket socket, SocketAwaitable awaitable)
        {
            return OperationAsync(socket, awaitable, ConnectAsyncHandler);
        }

        public static SocketAwaitable ReceiveSocketAsync(this Socket socket, SocketAwaitable awaitable)
        {
            return OperationAsync(socket, awaitable, ReceiveAsyncHandler);
        }

        public static SocketAwaitable SendSocketAsync(this Socket socket, SocketAwaitable awaitable)
        {
            return OperationAsync(socket, awaitable, SendAsyncHandler);
        }

        static SocketAwaitable OperationAsync(this Socket socket, SocketAwaitable awaitable, SocketOperationDelegate socketFunc)
        {
            awaitable.StartOperation();

            if (!socketFunc(socket, awaitable))
            {
                awaitable.CompleteOperation();
            }

            return awaitable;
        }
    }

}
