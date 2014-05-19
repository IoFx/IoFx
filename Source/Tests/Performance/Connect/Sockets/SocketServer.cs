using System;

namespace Connect.Sockets
{
    partial class SocketServer : IServer
    {
        private readonly int _port;
        private SocketCommandArgs _socketArgs;
        
        public SocketServer(int port, SocketCommandArgs args)
        {
            _port = port;
            _socketArgs = args;
        }

        public IDisposable StartServer()
        {
            if (_socketArgs.Ack)
            {
                return FixedLenghtWithAck();
            }

            return SimpleReceiver();
        }      
    }
}
