using System;
using System.IoFx.ServiceModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Connect.WCF.Channels
{
    class NetTcpDC : TestScenario
    {
        public static MessageBuffer CreateMessage(Binding binding)
        {
            var obj = new SimpleObject()
            {
                Id = int.MaxValue,
                Value = "Hello World"
            };

            var message = Message.CreateMessage(binding.MessageVersion, "action", obj);
            return message.CreateBufferedCopy(int.MaxValue);
        }

        public override void Run()
        {
            if (ConnectArguments.IsServer())
            {
                StartServer();
            }
            else
            {
                StartClients();
            }
        }

        private IDisposable StartClients()
        {
            var binding = new NetTcpBinding() { Security = { Mode = SecurityMode.None } };

            var clientmanager = new DuplexChannelManager(
                ConnectArguments.ConnectionLimit,
                ConnectArguments.MessageRate,
                binding,
                ConnectArguments.CreateNetTcpAddress(),
                CreateMessage(binding));

            return clientmanager.Start();
        }

        public IDisposable StartServer()
        {
            var monitor = new ConnectionRateMonitor();
            var binding = new NetTcpBinding { Security = { Mode = SecurityMode.None } };
            var listener = binding.Start(this.ConnectArguments.CreateNetTcpAddress());
            Console.WriteLine("Listening on {0} for {1}", listener.Uri, typeof(SimpleObject).Name);

            listener.GetChannels()
                .SubscribeOn(System.Reactive.Concurrency.ThreadPoolScheduler.Instance)
                .Subscribe(c =>
                {
                    monitor.OnConnect();
                    c.GetMessages()
                        .Subscribe(
                            m =>
                            {
                                monitor.OnMessage();
                                var obj = m.GetBody<SimpleObject>();
                            },
                        ex => monitor.OnDisconnect(),
                        monitor.OnDisconnect);
                });

            monitor.Start();
            return Disposable.Create(listener.Abort);
        }
    }


    [DataContract]
    internal class SimpleObject
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
}
