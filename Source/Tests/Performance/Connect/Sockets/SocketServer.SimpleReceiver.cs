using System;
using System.IoFx.Sockets;

namespace Connect.Sockets
{
    partial class SocketServer : IServer
    {
        private IDisposable SimpleReceiver()
        {
            var sockets = SocketEvents.GetTcpStreamSockets(_port);
            var monitor = new ConnectionRateMonitor();

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

            return sockets;
        }       
    }
}
