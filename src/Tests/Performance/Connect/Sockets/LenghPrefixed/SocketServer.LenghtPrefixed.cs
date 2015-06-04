using System;
using IoFx.Framing;
using IoFx.Sockets;

namespace Connect.Sockets
{
    partial class SocketServer : IServer
    {
        private IDisposable FixedLenghtWithAck()
        {
            var sockets = SocketEvents.GetTcpStreamSockets(_port);
            var monitor = new ConnectionRateMonitor();

            sockets.GetConnections()
                   .Subscribe(connection =>
            {
                monitor.OnConnect();
                connection
                    .ToLengthPrefixed()
                    .Subscribe(context =>
                    {                       
                        var response = HandleMessage(context.Message);
                        monitor.OnMessage();
                        context.Publish(response);
                    },
                    ex => monitor.OnDisconnect(),
                    monitor.OnDisconnect);
            });

            monitor.Start();

            return sockets;
        }

        private byte[] HandleMessage(byte[] buffer)
        {
            return BitConverter.GetBytes(buffer.Length);
        }
    }
}
