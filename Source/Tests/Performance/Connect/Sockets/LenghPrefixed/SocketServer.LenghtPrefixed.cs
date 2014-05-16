using System;
using System.IoFx.Framing;
using System.IoFx.Sockets;

namespace Connect.Sockets
{
    partial class SocketServer : IServer
    {
        private IDisposable FixedLenghtWithAck()
        {
            var sockets = SocketEvents.GetTcpStreamSockets(_port);
            var monitor = new ConnectionRateMonitor();

            sockets.GetConnections().Subscribe(connection =>
            {
                monitor.OnConnect();
                connection
                    .ToLengthPrefixed()
                    .Subscribe(context =>
                    {
                        monitor.OnMessage();
                        var response = HandleMessage(context.Message);
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
