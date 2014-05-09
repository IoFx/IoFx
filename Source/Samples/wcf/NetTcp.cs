using System;
using System.IoFx.ServiceModel;
using System.ServiceModel;

namespace Samples.wcf
{
    class NetTcp
    {
        public static void Main(string args)
        {
            var binding = new NetTcpBinding {Security = {Mode = SecurityMode.None}};
            var listener = binding.Start("net.tcp://localhost:8080/");
            Console.WriteLine("Listening on " + listener.Uri);

            listener.GetChannels()
                .Subscribe(channel =>
                    channel.GetMessages()
                        .Subscribe(
                            m => Console.WriteLine("Message Received."),
                            ex => Console.WriteLine(ex.Message),
                            () => Console.WriteLine("Disconnected")));

        }

    }
}
