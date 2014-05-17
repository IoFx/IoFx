using System;
using System.IoFx;
using System.IoFx.Connections;
using System.Reactive;
using System.Reactive.Linq;

namespace Samples
{
    class StringDispatcher
    {

        public static void SimpleCharToByteTest()
        {
            Func<char, byte> charToByte = c => (byte)(int)c;

            var transportRequests = "ABCDEFG".ToCharArray().ToObservable<char>();
            var transportSink = Observer.Create<Context<char>>(c => Console.WriteLine("Received : " + c.Message));


            //TODO: Fix string dispatcher #6
            /*
            var units = transportRequests
                        .ToContexts()
                        .ToConnection(charToByte);


            Func<byte, byte> operation = input => (byte)(input + (byte)1);            

            transportRequests.Subscribe(e => Console.WriteLine(e));
            Console.ReadLine();
             * */
        }
    }
}
