using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IoFx;
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
            var transportSink = Observer.Create<IoUnit<char>>(c => Console.WriteLine("Received : " + c.Unit));

            var units = transportRequests
                        .ToIoUnits()
                        .ToPipeline(charToByte);


            Func<byte, byte> operation = input => (byte)(input + (byte)1);
            


            transportRequests.Subscribe(e => Console.WriteLine(e));
            Console.ReadLine();
        }
    }
}
