namespace SM.Rx.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SM.Rx.Test.ServiceModel.TypedContracts;
    using System;
    using System.Diagnostics;
    using System.Reactive.Concurrency;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [TestClass]
    public class ChannelModelTests
    {
        static string address = "net.tcp://localhost:8080";
        static MessageVersion version = new NetTcpBinding().MessageVersion;

        [TestMethod]
        public void AcceptChannel()
        {
            var binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.None;

            var listener = binding.Start(address);
            Scheduler.CurrentThread.Catch<Exception>((e) =>
                {
                    Trace.WriteLine(e.Message);
                    return true;
                });
            try
            {
                listener
                    .OnConnect()
                    .Subscribe(
                    channel =>
                    {

                        var responses = channel.Select(  message =>
                        {
                            var input  = message.GetBody<string>();
                            return Message.CreateMessage(version, "", "Echo:" + input);
                        });

                        channel.Consume(responses);

                        (from message in channel
                         let input = message.GetBody<string>()
                         select Message.CreateMessage(version, "", "Echo:" + input))
                         .Subscribe(channel);
                    });

                SendMessages();
                System.Threading.Thread.Sleep(1000000);
            }
            finally
            {
                listener.Close();
            }
        }

        private static void SendMessages()
        {
            var binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.None;
            var factory = binding.BuildChannelFactory<IDuplexSessionChannel>(binding);
            factory.Open();
            var proxy = factory.CreateChannel(new EndpointAddress(address));
            proxy.Open();
            //proxy.ReceiveOn().Subscribe((message) =>
            //{
            //    Console.WriteLine("Received :" + message.GetBody<string>());                
            //});
            var msg = "Hello";
            proxy.Send(Message.CreateMessage(version, "Test", msg));
            var message = proxy.Receive();
            Assert.AreEqual("Echo:" + msg, message.GetBody<string>());
        }

        [TestMethod]
        public void CustomerOrderDataContracts()
        {
            var r = TypedServices.StartService();
            var response = TypedServices.Invoke("TestName");
            Assert.AreEqual(response, "TestName:Order");
            r.Dispose();
        }

        [TestMethod]
        public void CustomerOrderDataContractsUsingChannels()
        {
            var r = TypedServices.ChannelModelDispatcher();
            var res = TypedServices.Invoke("TestName");
            Assert.AreEqual(res, "TestName:Order");
            r.Dispose();
        }
    }
}
