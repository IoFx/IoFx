using System.Threading.Tasks;

namespace System.IoFx.Connections
{
    class ConnectionAcceptor<TConnection, TFrame> : IListener<IConnection<TFrame>>
    {
        private readonly IListener<TConnection> _listener;
        private readonly IObservable<IConnection<TFrame>> _connections;

        public ConnectionAcceptor(IListener<TConnection> listener, IObservable<IConnection<TFrame>> connections)
        {
            _listener = listener;
            _connections = connections;
        }

        public Task Start()
        {
            return _listener.Start();
        }

        public void Dispose()
        {
            _listener.Dispose();
        }

        public IDisposable Subscribe(IObserver<IConnection<TFrame>> observer)
        {
            return _connections.Subscribe(observer);
        }
    }
}