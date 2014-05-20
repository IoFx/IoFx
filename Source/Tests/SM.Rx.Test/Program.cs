using System;
using IoFx.ServiceModel;
using System.Reactive.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using SM.Rx.Test.ServiceModel.TypedContracts;

namespace IoFx.Test
{
    class Program
    {
        static string address = "net.tcp://localhost:8080";
        //static MessageVersion version = new NetTcpBinding().MessageVersion;

        static void Main(string[] args)
        {
            //ChannelComposer.Test();
            //ChannelTest();
            MessageTest();
            //ContractTest();            
            Console.ReadLine();
        }

        private static void ContractTest()
        {
            TypedServices.StartService();
            TypedServices.Invoke();
        }

        private static void MessageTest()
        {
            NetTcpBinding b = new NetTcpBinding();
            var listener = b.Start(address);
            listener
                .OnConnect()
                .Do(c => Console.WriteLine("Channel accepted"))
                .OnMessage()
                .Do(m => Console.WriteLine("Received " + m.Message.Headers.Action))
                .Subscribe(
                 r =>
                 {
                     var input = r.Message.GetBody<string>();
                     r.Publish(Message.CreateMessage(b.MessageVersion, "", "Echo:" + input));
                     r.Publish(Message.CreateMessage(b.MessageVersion, "", "Echo:" + input));
                 });
        }

        private static void ChannelConnectTest()
        {
            var netTcpBibnding = new NetTcpBinding();
            var listener = netTcpBibnding.Start(address);
            listener.GetChannels().Subscribe(channel => {
                channel.GetMessages().Subscribe(message =>
                    {
 
                    });
            });
            listener.OnConnect(channel =>
                {
                    channel.Subscribe(message =>
                    {
                        var input = message.GetBody<string>();
                        var response = Message.CreateMessage(netTcpBibnding.MessageVersion, "", "Echo:" + input);
                        channel.Publish(message);
                    });
                });
        }

        private static void AcceptChannelTest()
        {
            var binding = new NetTcpBinding();
            var listener = binding.Start(address);
            var connections = from channel in listener.GetChannels()
                           select new
                           {
                               Messages = channel.GetMessages(),
                               Response = channel.GetConsumer()
                           };

            connections.Subscribe(item =>
            {
                item.Response.Publish(Message.CreateMessage(binding.MessageVersion, "Test", "Echo:" + "Connected"));

                /*      
                      (from message in channel.Messages
                       let input = message.GetBody<string>()
                       from response in new[] { Message.CreateMessage(version, "", "Echo:" + input) }
                       select response)
                      .Subscribe(channel.Response);
                
                 */
                item.Messages.Subscribe((message) =>
                {
                    var input = message.GetBody<string>();
                    var output = Message.CreateMessage(binding.MessageVersion, "", "Echo:" + input);
                    item.Response.Publish(output);
                });
            });
        }
    }
}
