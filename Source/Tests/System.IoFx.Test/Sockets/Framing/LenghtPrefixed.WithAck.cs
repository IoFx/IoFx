using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using IoFx.Framing;
using IoFx.Test.Utility;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading;
using IoFx.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace IoFx.Test.Sockets.Framing
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
                SendAndReceiveMultiplePayloadAck(2);
                SendAndReceiveMultiplePayloadAck(100);
            }
            finally
            {
                listener.Dispose();
            }
        }

        private int _sequence;
        public IDisposable StartServerWithAck(Action<byte[]> assert)
        {
            var listener = SocketEvents.CreateTcpStreamListener(5050);

            return listener
                .Subscribe(connection =>
                {
                    var contexts = connection
                        .ToLengthPrefixed()
                        .Subscribe(m =>
                        {
                            Console.WriteLine("Server Received {0} bytes", m.Message.Length);
                            if (assert != null)
                                assert(m.Message);
                            var next = Interlocked.Increment(ref _sequence);
                            var sequence = BitConverter.GetBytes(next);
                            Contract.Assert(sequence.Length == 4);
                            m.Publish(sequence);

                        },
                    ex => Console.WriteLine(ex.Message),
                    () => Console.WriteLine("Disconnected"));
                });
        }

        public void SendAndReceiveAck()
        {
            Socket s = GetConnectedSocket();
            var sender = s.CreateSender();
            var chars = GetCharPayload(1024);
            sender.Publish(chars);
            var data = s.CreateReceiver().FirstAsync();
            var result = data.Wait();
            if (result.Count != 4)
            {
                throw new InvalidOperationException("Incorrect number of bytes recieved.");
            }
            Console.WriteLine("Data Received " + BitConverter.ToInt32(result.Array, 0));
        }

        private static Socket GetConnectedSocket()
        {
            Socket s = new Socket(SocketType.Stream, ProtocolType.Tcp);
            s.Connect(new DnsEndPoint("localhost",5050));
            Console.Write("Connected to {0} : {1}", s.RemoteEndPoint.AddressFamily, s.RemoteEndPoint);
            return s;
        }

        void SendAndReceiveMultiplePayloadAck(int count)
        {
            Socket socket = GetConnectedSocket();
            var connection = socket.ToConnection();
            var chars = GetCharPayload(1024);
            var buffer = chars.Array;
            var payload = buffer;
            for (int i = 1; i < count; i++)
            {
                payload = payload.Prepend(buffer);
            }
            connection.Publish(new ArraySegment<byte>(payload));
            CountdownEvent countdown = new CountdownEvent(count);
            var data = connection.
                ToLengthPrefixed().
                Subscribe(result =>
            {
                Contract.Assert(result.Message.Length == 4);
                Console.Write("Receive Message of size {0} ", result.Message.Length);
                Console.WriteLine("Data Received " + BitConverter.ToInt32(result.Message, 0));
                countdown.Signal();
            });

            if (Debugger.IsAttached)
            {
                countdown.Wait();
            }
            else
            {
                countdown.Wait(TimeSpan.FromSeconds(5));
            }
            socket.Close();
            Assert.AreEqual(countdown.CurrentCount, 0);
        }
    }
}
