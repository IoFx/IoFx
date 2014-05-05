using System;
using System.Reactive.Linq;
using System.Threading;

namespace Connect
{
    public class ConnectionRateMonitor
    {
        int _connectionCount = 0;
        int _messageCount = 0;

        public void OnConnect()
        {
            Interlocked.Increment(ref _connectionCount);
        }

        public void OnDisconnect()
        {
            Interlocked.Decrement(ref _connectionCount);
        }

        public void OnMessage()
        {
            Interlocked.Increment(ref _messageCount);
        }

        public IDisposable Start()
        {
            return Observable.Interval(TimeSpan.FromSeconds(5)).TimeInterval()
                .Subscribe(_ =>
                {
                    double rate = Interlocked.Exchange(ref _messageCount, 0) / 5.0;
                    var current = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("server: Active: ");
                    Console.Write(_connectionCount);
                    Console.Write("\tMsg/sec: ");
                    Console.WriteLine(rate);
                    Console.ForegroundColor = current;
                });
        }
    }
}