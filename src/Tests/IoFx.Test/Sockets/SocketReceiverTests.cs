using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using IoFx.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IoFx.Test.Sockets
{
    [TestClass]
    public class SocketReceiverTests
    {
        [TestMethod]
        public void SocketReceive()
        {
            const int port = 5050;
            var sockets = SocketEvents.GetTcpStreamSockets(port);
            try
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

                var sender = SocketTestUtility.Connect(port);
                var bytes = 0;

                for (int i = 0; i < 100; i++)
                {
                    bytes += SocketTestUtility.Send(sender, "This is a test<EOF>");
                }
                sender.Close(5);
                tcs.Task.Wait(Defaults.MediumTestWaitTime);
                Console.WriteLine("Sent Bytes: {0} Received Bytes {1}", bytes, count);
                Assert.IsTrue(count == bytes);
            }
            finally
            {
                sockets.Dispose();
            }
        }

        [TestMethod]
        public void SocketReceive1Gb()
        {
            const int port = 5050;
            var payload = new byte[1024];
            const int totalSize = 1024 * 1024 * 1024;
            var random = new Random();
            random.NextBytes(payload);

            using (var sockets = SocketEvents.GetTcpStreamSockets(port))
            {

                var tcs = new TaskCompletionSource<object>();
                int count = 0;
                sockets.Subscribe(s =>
                {
                    var data = s.CreateReceiver();
                    data.Do(d => count += d.Count)
                        .TakeWhile(_ => count <= totalSize)
                        .Subscribe(_ => { },
                            tcs.SetException,
                            () => tcs.SetResult(null));
                });

                var sender = SocketTestUtility.Connect(port);
                var bytes = 0;

                do
                {
                    bytes += sender.Send(payload);
                } while (bytes < totalSize);

                sender.Close(5);
                tcs.Task.Wait(Defaults.MediumTestWaitTime);
                Console.WriteLine("Sent Bytes: {0} Received Bytes {1}", bytes, count);
                Assert.IsTrue(count == totalSize);
            }
        }


    }
}
