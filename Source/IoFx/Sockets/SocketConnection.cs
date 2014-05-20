using System;
using System.Net.Sockets;
using System.Threading;
using IoFx.Connections;

namespace IoFx.Sockets
{
    class SocketConnection : IDisposable
    {
        private readonly Socket _socket;
        private int _disposed;

        public static IDisposableConnection<ArraySegment<byte>> Create(Socket socket)
        {
            var receiver = socket.CreateReceiver();
            var sender = socket.CreateSender();
            var disposable = new SocketConnection(socket);
            return new DisposableConnection<ArraySegment<byte>>(receiver, sender, disposable);            
        }
        private SocketConnection(Socket receiveSocket)
        {
            _socket = receiveSocket;
        }

        void Dispose(bool shutdown)
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                if (shutdown && _socket.Connected)
                {
                    _socket.Shutdown(SocketShutdown.Send);
                }

                _socket.Close();
                _socket.Dispose();
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
    }
}