using System.IoFx.Connections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace System.IoFx.Tests
{
    [TestClass]
    public class CustomDispatcher
    {
        [TestMethod]
        public void Dispatcher()
        {
            List<byte[]> outputs = new List<byte[]>();
            var inputs = new string[] { "A", "BB", "CCC", "DDDD" };
            Func<byte[], string> decode = t => Encoding.ASCII.GetString(t);
            Func<string, byte[]> encode = s => Encoding.ASCII.GetBytes(s);
            var testMessages = inputs
                    .Select(encode)
                    .ToObservable();

            var stringDispatcher = new StringDispatcher();

            stringDispatcher.Outputs.Subscribe((m) =>
            {
                Console.WriteLine("Received " + m);
                stringDispatcher.Inputs.Publish(m + "!");
            });

            var transport = new InMemoryTransport(testMessages);
            var byteRequests = transport.Publish(); // ensure all subscriptions are done before starting. 


            // Get strings from bytes
            var stringRequest = byteRequests.Select(decode);


            // Consume strings
            stringDispatcher.Consume(stringRequest);

            // Do operations
            var results = stringDispatcher.Inputs.Select(s => s + "$");

            // Get bytes from strings 
            var byteResponses = results.Select(encode).Do(r => outputs.Add(r));

            // transport consumes the byte outputs. 
            transport.Consume(byteResponses);


            //Start the transport
            byteRequests.Connect();

            Assert.AreEqual(inputs.Length, outputs.Count);

            for (int i = 0; i < outputs.Count; i++)
            {
                byte[] o = outputs[i];
                Assert.AreEqual(inputs[i] + "!$", decode(o));
            }
        }



        class StringDispatcher : Composer<string, string>
        {
            public StringDispatcher()
                : base(new IoSink<string>(), new IoSink<string>())
            { }
        }

        class InMemoryTransport : Connection<byte[]>
        {
            public InMemoryTransport(IObservable<byte[]> outputs)
                : base(outputs, new IoSink<byte[]>())
            {

            }
        }

        class IoSink<T> : IConnection<T>
        {
            private readonly Subject<T> _sink = new Subject<T>();

            public IDisposable Subscribe(IObserver<T> observer)
            {
                return _sink.Subscribe(observer);
            }

            public void Publish(T item)
            {
                _sink.OnNext(item);
            }
        }
    }
}
