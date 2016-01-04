using System;
using IoFx.Sockets;
using IoFx.Test.Utility;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IoFx.Test.Sockets
{
    
    public class SocketAcceptTest
    {
        [Fact]
        public void SocketConnect()
        {
            const int port = 5050;
            const int clientCount = 10;
            var listener = SocketEvents.GetTcpStreamSockets(port);
            var countdown = new CountdownEvent(clientCount);
            try
            {
                
                var tcs = new TaskCompletionSource<object>();
                int count = 0;
                listener.Subscribe(s =>
                {
                    count++;
                    countdown.Signal();
                    s.Close();
                },
                tcs.SetException,
                () => tcs.TrySetResult(null));

                for (int i = 0; i < clientCount; i++)
                {
                    SocketTestUtility.Connect(port);
                }

                countdown.WaitEx();
                Xunit.Assert.True(count == clientCount);
            }
            finally
            {
                listener.Dispose();
            }
        }


    }
}
