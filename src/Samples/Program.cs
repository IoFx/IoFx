using Samples.sockets;
using System;
using System.Threading.Tasks;

namespace Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            //  StringDispatcher.SimpleCharToByteTest();
            var server = SimpleSocketServer.StartServer();
            Task t = null;
            int size = 1024 * 1024;
            Console.WriteLine("Sending: " + size);
            t = SimpleSocketServer.SendData(size, 5);
            t.Wait();

            Console.ReadLine();
        }
    }
}
