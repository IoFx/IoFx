using System;
using System.Collections.Generic;
using System.IoFx.Sockets;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IoFx.Framing;
using System.Collections.Concurrent;
using System.IoFx.Connections;
using System.Diagnostics;

namespace Connect.Sockets.LenghPrefixed
{
    class SocketClientWithAck : IClientManager
    {
        private System.Net.IPEndPoint _endpoint;
        private ConnectArgs _args;
        private SocketCommandArgs _socketArgs;
        private List<IConnection<Context<byte[]>, byte[]>> _clients;
        private IEnumerator<IConnection<Context<byte[]>, byte[]>> _connections;
        private byte[] _payload;
        private ConnectionRateMonitor _monitor;

        public SocketClientWithAck(ConnectArgs args, SocketCommandArgs socketArgs)
        {
            _args = args;
            _socketArgs = socketArgs;
            _endpoint = SocketUtility.GetFirstIpEndPoint(_args.Server, _args.Port);
            _clients = new List<IConnection<Context<byte[]>, byte[]>>();
            _connections = GetNext().GetEnumerator();
            _payload = GetChars(100);
            _monitor = new ConnectionRateMonitor();
        }

        IEnumerable<IConnection<Context<byte[]>, byte[]>> GetNext()
        {
            int i = 0;
            while (true)
            {
                yield return _clients[i];
                i = (i + 1) % _args.ConnectionLimit;
            }
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
                        tasks[i] = Task.Run(() => Connect(pending));
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

        public async Task Connect(CountdownEvent pending)
        {
            var connection = await SocketEvents.CreateConnection(_args.Server, _args.Port);
            _monitor.OnConnect();
            var encodedConnection = connection.ToLengthPrefixed();
            _clients.Add(encodedConnection);
            pending.Signal();
            encodedConnection.Subscribe(this.HandleResponse);
        }

        private void SendMessage(long obj)
        {
            for (int i = 0; i < _args.MessageRate; i++)
            {
                _connections.MoveNext();
                var client = _connections.Current;

                client.Publish(_payload);

            }
        }

        private void HandleResponse(Context<byte[]> obj)
        {
            Trace.Assert(obj.Message.Length == 4);
            BitConverter.ToInt32(obj.Message, 0);
            _monitor.OnMessage();
        }

        static byte[] GetChars(int size)
        {
            var chars = Enumerable.Range(0, size)
                .Select(i => (char)((i % 26) + 65))
                .ToArray();
            return Encoding.ASCII.GetBytes(chars);
        }
    }
}
