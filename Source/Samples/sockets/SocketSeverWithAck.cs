using System;
using System.Diagnostics.Contracts;
using System.IoFx.Framing;
using System.IoFx.Sockets;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples.sockets
{
    class SocketSeverWithAck
    {

        public static IDisposable StartServer()
        {
            var listener = SocketObservable.CreateTcpStreamListener(5050);

            return listener
                .Subscribe(connection =>
                {
                    var messages = connection.ToLengthPrefixed();
                    messages.Subscribe(m =>
                    {  
                            Console.WriteLine("Received {0} bytes", m.Message.Length);
                            m.Publish(m.Message);
                    },

                    ex => Console.WriteLine(ex.Message),
                    () => Console.WriteLine("Disconnected"));
                });
        }

        public async static Task SendData(int size = 5, int repeat = 1)
        {
            byte[] preamble = BitConverter.GetBytes(size);
            var data = Encoding.ASCII.GetBytes(GetChars(size));
            Contract.Assert(preamble.Length == 4);

            var buffer = new byte[size + preamble.Length];
            Buffer.BlockCopy(preamble, 0, buffer, 0, preamble.Length);
            Buffer.BlockCopy(data, 0, buffer, preamble.Length, data.Length);
            var payload = new ArraySegment<byte>(buffer);
            var sender = await SocketObservable.CreateTcpStreamSender("localhost", 5050);
            for (int i = 0; i < repeat; i++)
            {
                sender.Publish(payload);
            }

            sender.Dispose();
        }


        static char[] GetChars(int size)
        {
            var chars = Enumerable.Range(0, size).Select(i =>
                {
                    return (char)((i % 26) + 65);
                }).ToArray();

            return chars;
        }
    }
}
