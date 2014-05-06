using System;
using System.Reactive.Disposables;
using System.ServiceModel;

namespace Connect.WCF.Channels
{
    class TcpChannelServer : IServer
    {
        private readonly string _address;

        public TcpChannelServer(string address)
        {
            _address = address;
        }

        public IDisposable StartServer()
        {
            ConnectionRateMonitor monitor = new ConnectionRateMonitor();
            var binding = new NetTcpBinding { Security = { Mode = SecurityMode.None } };
            var listener = binding.Start(_address);
            Console.WriteLine("Listening on " + listener.Uri);

            listener.GetChannels()
                .SubscribeOn(System.Reactive.Concurrency.ThreadPoolScheduler.Instance)
                .Subscribe(c =>
                {
                    monitor.OnConnect();
                    c.GetMessages()
                        .Subscribe(
                        m => monitor.OnMessage(),
                        ex => monitor.OnDisconnect(),
                        monitor.OnDisconnect);
                });

            monitor.Start();

            return Disposable.Create(listener.Abort);
        }
    }
}
