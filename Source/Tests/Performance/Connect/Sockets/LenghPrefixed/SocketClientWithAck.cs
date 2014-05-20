using System;
using System.Collections.Generic;
using System.Diagnostics;
using IoFx.Connections;
using IoFx.Framing;
using IoFx.Sockets;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Connect.Sockets.LenghPrefixed
{
    class SocketClientWithAck : IClientManager
    {
        private readonly ConnectArgs _args;
        private readonly List<IConnection<Context<byte[]>, byte[]>> _clients;
        private readonly byte[] _payload;
        private readonly IEnumerator<IConnection<Context<byte[]>, byte[]>> _connectionEnumerator;
        private readonly ConnectionRateMonitor _monitor;

        public SocketClientWithAck(ConnectArgs args, SocketCommandArgs socketArgs)
        {
            _args = args;
            _clients = new List<IConnection<Context<byte[]>, byte[]>>();
            _connectionEnumerator = this.GetNext().GetEnumerator();
            _payload = GetChars(socketArgs.Size);
            _monitor = new ConnectionRateMonitor();
        }
    
        public IDisposable Start()
        {
            _monitor.Start();
            CountdownEvent pending = new CountdownEvent(_args.ConnectionLimit);
            var interval = Observable.Interval(TimeSpan.FromSeconds(1))
                .TakeWhile(_ => pending.CurrentCount > 0)
                .Subscribe(async _ =>
                {
                    var parallelCount = Math.Min(pending.CurrentCount, 10);

                    Task[] tasks = new Task[parallelCount];
                    for (int i = 0; i < parallelCount; i++)
                    {
                        tasks[i] = Task.Run(() => ConnectAndSubscribeAsync(pending));
                    }

                    Task.WaitAll(tasks);
                },
                ex =>
                {
                    Console.WriteLine(ex.Message);
                    Environment.Exit(1);
                });

            pending.Wait();

            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(this.SendMessage);

            return null;
        }

        IEnumerable<IConnection<Context<byte[]>, byte[]>> GetNext()
        {
            int i = 0;
            while (true)
            {
                if (i < 0 || i >= _clients.Count)
                {
                    string message = string.Format("Incorrect index {0} computation", i);
                    Console.WriteLine(message);
                    throw new InvalidOperationException(message);
                }

                yield return _clients[i];

                int next;
                int slot;
                do
                {
                    slot = i;
                    next = (i + 1) % _args.ConnectionLimit;
                } while (Interlocked.CompareExchange(ref i, next, slot) != slot);
            }
        }


        private async Task ConnectAndSubscribeAsync(CountdownEvent pending)
        {
            var connection = await SocketEvents.CreateConnection(_args.Server, _args.Port);
            _monitor.OnConnect();
            var encodedConnection = connection.ToLengthPrefixed();
            lock (_clients)
            {
                _clients.Add(encodedConnection);
            }
            pending.Signal();
            encodedConnection.Subscribe(this.HandleResponse);
        }

        private void HandleResponse(Context<byte[]> obj)
        {
            Trace.Assert(obj.Message.Length == 4);
            var length = BitConverter.ToInt32(obj.Message, 0);
            if (length != _payload.Length)
            {
                throw new InvalidOperationException();
            }
            _monitor.OnMessage();
        }

        private void SendMessage(long obj)
        {
            for (int i = 0; i < _args.MessageRate; i++)
            {
                _connectionEnumerator.MoveNext();
                var client = _connectionEnumerator.Current;

                client.Publish(_payload);
            }
        }

        private static byte[] GetChars(int size)
        {
            var chars = Enumerable.Range(0, size)
                .Select(i => (char)((i % 26) + 65))
                .ToArray();
            return Encoding.ASCII.GetBytes(chars);
        }
    }
}
