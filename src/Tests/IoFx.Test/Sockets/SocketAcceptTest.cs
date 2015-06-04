using System;
using IoFx.Sockets;
using IoFx.Test.Utility;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IoFx.Test.Sockets
{
    [TestClass]
    public class SocketAcceptTest
    {
        [TestMethod]
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
                Assert.IsTrue(count == clientCount);
            }
            finally
            {
                listener.Dispose();
            }
        }


    }
}
