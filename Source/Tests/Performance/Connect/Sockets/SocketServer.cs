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

        public IDisposable StartServer()
        {
            var sockets = SocketObservable.AcceptTcpStream(_port);
            var monitor = new ConnectionRateMonitor();

            sockets.Subscribe(s =>
            {
                monitor.OnConnect();
                var receiver = s.GetData();
                receiver.Subscribe(
                    d => monitor.OnMessage(),
                    ex => monitor.OnDisconnect(),
                    monitor.OnDisconnect);
            });

            monitor.Start();

            return sockets;
        }      
    }
}
