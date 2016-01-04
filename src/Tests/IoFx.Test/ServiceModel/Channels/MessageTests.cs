using System;
using System.Diagnostics;
using IoFx.ServiceModel;
using System.Reactive.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IoFx.Test.ServiceModel.Channels
{
    
    public class MessageTests
    {
        string _address = "net.tcp://localhost:8080/test";

        [Fact]
        public void GetMessage()
        {
            var count = GetMessages(1);
            Xunit.Assert.Equal(count, 1);
        }

        [Fact]
        public void Get50KMessages()
        {
            const int expectedMessageCount = 50 * 1000;
            var count = GetMessages(expectedMessageCount);
            Xunit.Assert.Equal(count, expectedMessageCount);
        }

        public int GetMessages(int numberOfMessages)
        {
            var binding = new NetTcpBinding { Security = { Mode = SecurityMode.None } };
            var listener = binding.Start(_address);
            var tcs = new TaskCompletionSource<int>();
            try
            {
                int count = 0;
                var watch = new Stopwatch();
                watch.Start();
                listener.GetChannels().Subscribe(c =>
                {
                    Console.WriteLine("Channel accepted");
                    c.GetMessages().Subscribe(m =>
                    {
                        // Console.WriteLine("Message received");
                        var bytes = m.GetBody<byte[]>();
                        // Console.WriteLine(Encoding.ASCII.GetString(bytes));
                        if (Interlocked.Increment(ref count) == numberOfMessages)
                        {
                            tcs.SetResult(numberOfMessages);
                        }
                    },
                    tcs.SetException);
                }, tcs.SetException);

                SendMessages(binding, numberOfMessages, _address);

                Observable.Interval(TimeSpan.FromSeconds(1))
                    .TakeWhile(_ => count <= numberOfMessages)
                    .Subscribe(_ => Console.WriteLine("Received {0} messages at {1} messages/sec",
                        count,
                        count / watch.Elapsed.TotalSeconds));

                tcs.Task.Wait(Defaults.LongTestWaitTime);
                watch.Stop();
                var seconds = watch.Elapsed.TotalSeconds;
                Console.WriteLine("Received {0} messages in {1} seconds : {2} messages/sec",
                    count,
                    seconds,
                    count / seconds);

                return count;
            }
            finally
            {
                listener.Abort();
            }
        }

        private static async void SendMessages(Binding binding, int messageCount, string address)
        {

            var factory = binding.BuildChannelFactory<IDuplexSessionChannel>();
            try
            {
                factory.Open();
                var channel = factory.CreateChannel(new EndpointAddress(address));
                channel.Open();
                Console.WriteLine("Channel should be connected by now...");
                var sendBytes = Encoding.ASCII.GetBytes("Hello WCF over a channel");
                var message = Message.CreateMessage(binding.MessageVersion, "action", sendBytes);
                var clone = message.CreateBufferedCopy(int.MaxValue);

                for (int i = 0; i < messageCount; i++)
                {
                    await Task.Factory.FromAsync(channel.BeginSend(clone.CreateMessage(), null, null), channel.EndSend);
                }

                channel.Close();
            }
            finally
            {
                factory.Close();
            }
        }
    }
}
