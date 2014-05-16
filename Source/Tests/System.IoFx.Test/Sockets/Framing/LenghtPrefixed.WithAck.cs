using System;
using System.Diagnostics.Contracts;
using System.IoFx.Framing;
using System.IoFx.Sockets;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.IoFx.Test.Sockets.Framing
{    
    public partial class LenghtPrefixed
    {
        [TestMethod]
        public void LengthPrefixedWithAck()
        {
            var listener = StartServerWithAck(b => { });
            try
            {
                SendAndReceiveAck();
            }
            finally
            {
                listener.Dispose();
            }
        }

        private int _sequence;
        public IDisposable StartServerWithAck(Action<byte[]> assert)
        {
            var listener = SocketObservable.CreateTcpStreamListener(5050);
            
            return listener
                .Subscribe(connection =>
                {
                    var contexts = connection
                        .ToLengthPrefixed()
                        .Subscribe(m =>
                        {
                            Console.WriteLine("Received {0} bytes", m.Message.Length);
                            if (assert != null)
                                assert(m.Message);
                            var next = Interlocked.Increment(ref _sequence);
                            m.Publish(BitConverter.GetBytes(next));

                        },
                    ex => Console.WriteLine(ex.Message),
                    () => Console.WriteLine("Disconnected"));
                });
        }

        public void SendAndReceiveAck()
        {
            var ip = SocketHelpers.GetFirstIpEndPointFromHostName("localhost", 5050);
            Socket s = new Socket(SocketType.Stream, ProtocolType.Tcp);
            s.Connect(ip);
            var sender = s.CreateSender();
            var chars = GetCharPayload(1024);
            sender.Publish(new ArraySegment<byte>(chars));
            var data = s.CreateReceiver().FirstAsync();
            var result = data.Wait();
            Console.WriteLine(result.Count);
        }
    }
}
