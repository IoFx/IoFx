using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;

namespace Connect.WCF.Channels
{
    class DuplexChannelManager : IClientManager
    {
        private int _rate;
        private readonly int _connections;
        private IChannelFactory<IDuplexSessionChannel> _factory;
        private readonly EndpointAddress _address;

        private readonly Queue<IDuplexSessionChannel> _channels = new Queue<IDuplexSessionChannel>();
        private MessageBuffer _messageBuffer;
        private volatile bool _isDisposed;

        public DuplexChannelManager(int connections, int messageRate, Binding binding, string address, MessageBuffer messageBuffer = null)
        {
            _connections = connections;
            _rate = messageRate;
            _factory = binding.BuildChannelFactory<IDuplexSessionChannel>();
            _address = new EndpointAddress(address);
            _messageBuffer = messageBuffer ??  Message.CreateMessage(binding.MessageVersion, "TestAction", new byte[1024]).CreateBufferedCopy(int.MaxValue); ;
            _factory.Open();
        }

        public object ThisLock
        {
            get { return _channels; }
        }

        public IDisposable Start()
        {
            ConnectionRateMonitor monitor = new ConnectionRateMonitor();
            var connections = new Task[_connections];
            for (int i = 0; i < _connections; i++)
            {
                var channel = _factory.CreateChannel(_address);
                var openTask = Task.Factory.FromAsync(channel.BeginOpen, channel.EndOpen, null);
                openTask.ContinueWith(_ => monitor.OnConnect());
                _channels.Enqueue(channel);
                connections[i] = openTask;
            }

            Task.WaitAll(connections);

            double backlog = 0;
            double currentRate = 0;
            AsyncCallback cb = (iar) =>
                        {
                            IDuplexSessionChannel channel = (IDuplexSessionChannel)iar.AsyncState;
                            backlog = monitor.OnMessageEnd();
                            monitor.OnMessage();
                            channel.EndSend(iar);
                        };

            var load = Observable.Interval(TimeSpan.FromMilliseconds(1000))
                .TakeWhile(_ => !_isDisposed)
                .Where(_ => backlog <= _rate)
                .Subscribe(async _ =>
                {
                    int pending = _rate;

                    Action action = () =>
                    {
                        //Console.WriteLine("Thread ID" + System.Threading.Thread.CurrentThread.ManagedThreadId);
                        while (Interlocked.Decrement(ref pending) >= 0)
                        {
                            var channel = GetNextChannel();
                            monitor.OnMessageStart();
                            var iar = channel.BeginSend(_messageBuffer.CreateMessage(), cb, channel);
                        }
                    };

                    Parallel.Invoke(
                        new ParallelOptions()
                                    {
                                        MaxDegreeOfParallelism = Environment.ProcessorCount * 4,
                                    },
                                    action, action, action, action,
                                    action, action, action, action
                                    );
                },
                    ex => Console.WriteLine(ex.Message + " \n" + ex.StackTrace));

            var ratemonitor = monitor.Start();

            return Disposable.Create(() =>
            {
                // Terminate the timer as well.
                _isDisposed = true;
                load.Dispose();
                ratemonitor.Dispose();

                lock (ThisLock)
                {
                    while (_channels.Count > 0)
                    {
                        var c = _channels.Dequeue();
                        c.Abort();
                    }

                    _factory.Abort();
                }
            });
        }


        public IDuplexChannel GetNextChannel()
        {
            lock (ThisLock)
            {
                IDuplexSessionChannel channel = channel = _channels.Dequeue();
                _channels.Enqueue(channel);
                return channel;
            }
        }
    }
}
