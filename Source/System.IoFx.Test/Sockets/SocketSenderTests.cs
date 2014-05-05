using System.IoFx.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.IoFx.Test.Sockets
{
    [TestClass]
    public class SocketSenderTests
    {
        [TestMethod]
        public void SocketSend()
        {
            const int port = 5050;
            using (var sockets = SocketObservable.AcceptTcpStream(port))
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

                var socket = SocketHelpers.Connect(port);
                var sender = socket.CreateSender();
                var bytes = Encoding.ASCII.GetBytes("This is a test<EOF>");
                sender.OnNext(new ArraySegment<byte>(bytes, 0, bytes.Length));
                sender.OnCompleted();
                tcs.Task.Wait(TimeSpan.FromSeconds(5));
                Console.WriteLine("Sent Bytes: {0} Received Bytes {1}", bytes, count);
                Assert.IsTrue(count == bytes.Length);
            }
        }
    }
}
