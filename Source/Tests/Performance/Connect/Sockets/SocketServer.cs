using System;
using System.IoFx.Sockets;

namespace Connect.Sockets
{
    public class SocketServer : IServer
    {
        private readonly int _port;
        public SocketServer(int port)
        {
            _port = port;
        }

        public void StartServer()
        {
            var sockets = SocketObservable.AcceptTcpStream(_port);
            ConnectionRateMonitor monitor = new ConnectionRateMonitor();

            sockets.Subscribe(s =>
            {
                monitor.OnConnect();
                var receiver = s.CreateReceiver();
                receiver.Subscribe(
                    d => monitor.OnMessage(),
                    ex => monitor.OnDisconnect(),
                    monitor.OnDisconnect);
            });

            monitor.Start();
        }      
    }
}
