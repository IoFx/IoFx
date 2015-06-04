using System;
using System.Reactive.Linq;
using System.Threading;

namespace Connect
{
    public class ConnectionRateMonitor
    {
        private int _connectionCount = 0;
        private int _messageCount = 0;
        private int _backlog = 0;
        const double Interval = 5;
        private double _rate;

        public void OnConnect()
        {
            Interlocked.Increment(ref _connectionCount);
        }

        public void OnDisconnect()
        {
            Interlocked.Decrement(ref _connectionCount);
        }

        public double OnMessage()
        {
            return Interlocked.Increment(ref _messageCount) / Interval;
        }

        public void OnMessageStart()
        {
            Interlocked.Increment(ref _backlog);
        }

        public int OnMessageEnd()
        {
            return Interlocked.Decrement(ref _backlog);
        }

        public double Rate
        {
            get { return _rate; }
        }

        public IDisposable Start()
        {

            return Observable.Interval(TimeSpan.FromSeconds(Interval)).TimeInterval()
                .Subscribe(_ =>
                {
                    _rate = Interlocked.Exchange(ref _messageCount, 0) / Interval;
                    var current = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("Active: ");
                    Console.Write(_connectionCount);
                    Console.Write("\tMsg/sec: ");
                    Console.Write(_rate);

                    if (_backlog > 0)
                    {
                        Console.Write("\t\tBacklog: ");
                        Console.Write(_backlog);
                    }

                    Console.WriteLine();

                    Console.ForegroundColor = current;
                });
        }
    }
}