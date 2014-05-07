using System.IoFx.Connections;
using System.IoFx.ServiceModel;
using System.Reactive.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using SM.Rx.Test.ServiceModel.TypedContracts;

namespace System.IoFx.Test
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
                .Do(m => Console.WriteLine("Received " + m.Unit.Headers.Action))
                .Subscribe(
                 r =>
                 {
                     var input = r.Unit.GetBody<string>();
                     r.Publish(Message.CreateMessage(b.MessageVersion, "", "Echo:" + input));
                     r.Publish(Message.CreateMessage(b.MessageVersion, "", "Echo:" + input));
                 });
        }

        private static void ChannelConnectTest()
        {
            var netTcpBibnding = new NetTcpBinding();
            var listener = netTcpBibnding.Start(address);          
            listener.OnConnect(channel =>
                {
                    var outputs = channel.Select(message =>
                    {
                        var input = message.GetBody<string>();
                        return Message.CreateMessage(netTcpBibnding.MessageVersion, "", "Echo:" + input);
                    });

                    channel.Consume(outputs);
                });
        }

        private static void AcceptChannelTest()
        {
            var binding = new NetTcpBinding();
            var listener = binding.Start(address);
            var channels = from channel in listener.GetChannels()
                           select new
                           {
                               Messages = channel.GetMessages(),
                               Response = channel.ReplyOn()
                           };

            channels.Subscribe(channel =>
            {
                channel.Response.OnNext(Message.CreateMessage(binding.MessageVersion, "Test", "Echo:" + "Connected"));

                /*      
                      (from message in channel.Messages
                       let input = message.GetBody<string>()
                       from response in new[] { Message.CreateMessage(version, "", "Echo:" + input) }
                       select response)
                      .Subscribe(channel.Response);
                
                 */
                channel.Messages.Subscribe((message) =>
                {
                    var input = message.GetBody<string>();
                    var output = Message.CreateMessage(binding.MessageVersion, "", "Echo:" + input);
                    channel.Response.OnNext(output);
                });
            });
        }
    }
}
