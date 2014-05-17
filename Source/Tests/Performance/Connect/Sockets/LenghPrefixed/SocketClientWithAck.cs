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

namespace Connect.Sockets.LenghPrefixed
{
    class SocketClientWithAck : IClientManager
    {
        private System.Net.IPEndPoint _endpoint;
        private ConnectArgs _args;
        private SocketCommandArgs _socketArgs;
        private ConcurrentQueue<IConnection<Context<byte[]>>> _clients;

        public SocketClientWithAck(ConnectArgs args, SocketCommandArgs socketArgs)
        {
            _args = args;
            _socketArgs = socketArgs;
            _endpoint = SocketUtility.GetFirstIpEndPoint(_args.Server, _args.Port);
            _clients = new ConcurrentQueue<IConnection<Context<byte[]>>>();
        }


        public IDisposable Start()
        {
            CountdownEvent pending = new CountdownEvent(_args.ConnectionLimit);
            var interval = Observable.Interval(TimeSpan.FromSeconds(1))
                .TakeWhile(_ => pending.CurrentCount > 0)
                .Subscribe(async _ =>
                {
                    var connection = await SocketEvents.CreateConnection(_args.Server, _args.Port);
                    var encodedConnection = connection.ToLengthPrefixed();
                    pending.Signal();
                    encodedConnection.Subscribe(this.HandleResponse);                     
                });

            pending.Wait();

            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(this.SendMessage);

            return null;
        }

        private void SendMessage(long obj)
        {
            
        }

        private void HandleResponse(Context<byte[]> obj)
        {
            BitConverter.ToInt32(obj.Message, 0);
        }
    }
}
