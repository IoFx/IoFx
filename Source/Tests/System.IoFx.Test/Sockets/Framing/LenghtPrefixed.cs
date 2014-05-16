using System;
using System.Diagnostics.Contracts;
using System.IoFx.Framing;
using System.IoFx.Sockets;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.IoFx.Test.Sockets.Framing
{
    [TestClass]
    public partial class LenghtPrefixed
    {
        [TestMethod]
        public void AsciiEncoded4ByteTest()
        {
            var tcs = new TaskCompletionSource<bool>();
            Action<byte[]> t = buffer =>
            {
                try
                {
                    var result = Encoding.ASCII.GetString(buffer);
                    Console.WriteLine("Received " + result);
                    bool valid = CheckPayload(result.ToCharArray());
                    tcs.SetResult(valid);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            };

            using (var server = StartServer(t))
            {
                SendData(5, 1);
                tcs.Task.Wait(Defaults.ShortTestWaitTime);
            }

            Assert.IsTrue(tcs.Task.Result);
        }

        [TestMethod]
        public void AsciiEncoded5_1MbbyteTest()
        {
            var tcs = new TaskCompletionSource<int>();
            const int expecteSize = 1024 * 1024;
            const int expectedCount = 5;

            int count = 0;
            Action<byte[]> t = buffer =>
            {
                try
                {
                    var result = Encoding.ASCII.GetString(buffer);                  
                    bool valid = CheckPayload(result.ToCharArray());
                    count++;

                    if (result.Length != expecteSize)
                    {
                        tcs.SetResult(-1);
                        return;
                    }

                    if (count >= expectedCount)
                    {
                        tcs.SetResult(count);
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            };

            using (var server = StartServer(t))
            {
                SendData(expecteSize, expectedCount);
                tcs.Task.Wait(Defaults.ShortTestWaitTime);
            }

            Assert.IsTrue(tcs.Task.Result == expectedCount);
        }

        public IDisposable StartServer(Action<byte[]> assert)
        {

            var listener = SocketEvents.CreateTcpStreamListener(5050);

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
                    },
                    ex => Console.WriteLine(ex.Message),
                    () => Console.WriteLine("Disconnected"));
                });
        }



        public async static Task SendData(int size = 5, int repeat = 1)
        {
            var payload = GetCharPayload(size);            
            var sender = await SocketEvents.CreateTcpStreamSender("localhost", 5050);
            for (int i = 0; i < repeat; i++)
            {
                sender.Publish(payload);
            }

            sender.Dispose();
        }

        private static ArraySegment<byte> GetCharPayload(int size)
        {
            byte[] preamble = BitConverter.GetBytes(size);
            var data = Encoding.ASCII.GetBytes(GetChars(size));
            Contract.Assert(preamble.Length == 4);
            var buffer = new byte[size + preamble.Length];
            Buffer.BlockCopy(preamble, 0, buffer, 0, preamble.Length);
            Buffer.BlockCopy(data, 0, buffer, preamble.Length, data.Length);
            return new ArraySegment<byte>(buffer);
        }


        static char[] GetChars(int size)
        {
            var chars = Enumerable.Range(0, size)
                .Select(i => (char)((i % 26) + 65))
                .ToArray();
            return chars;
        }

        /// <summary>
        /// Function to check if the characters are in order. 
        /// ABCDEF.....
        /// </summary>
        /// <returns></returns>
        private static bool CheckPayload(char[] input)
        {
            if (input.Length > 1)
            {
                for (int i = 0; i < input.Length - 1; i++)
                {
                    int expected = ((input[i] - 65 + 1) % 26) + 65;
                    if (input[i + 1] != expected)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
