using System;
using System.Text;
using System.Threading.Tasks;
using IoFx.Sockets;
using Xunit;

namespace IoFx.Test.Sockets
{
    
    public class SocketSenderTests
    {
        [Fact]
        public void SocketSend()
        {
            const int port = 5050;
            using (var sockets = SocketEvents.GetTcpStreamSockets(port))
            {

                var tcs = new TaskCompletionSource<object>();
                int count = 0;
                sockets.Subscribe(s =>
                {
                    var data = s.CreateReceiver();
                    data.Subscribe(
                        d => count += d.Count,
                        tcs.SetException,
                        () => tcs.SetResult(null));
                });

                var socket = SocketTestUtility.Connect(port);
                var sender = socket.CreateSender();
                var bytes = Encoding.ASCII.GetBytes("This is a test<EOF>");
                sender.Publish(new ArraySegment<byte>(bytes, 0, bytes.Length));
                sender.Dispose();
                tcs.Task.Wait(Defaults.MediumTestWaitTime);
                Console.WriteLine("Sent Bytes: {0} Received Bytes {1}", bytes, count);
                Xunit.Assert.True(count == bytes.Length);
            }
        }
    }
}
