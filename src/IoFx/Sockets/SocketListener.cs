using System;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using IoFx.Connections;
using IoFx.Tracing;

namespace IoFx.Sockets
{
    class SocketListener : IListener<Socket>
    {
        private Socket _listenerSocket;
        private readonly TaskCompletionSource<Socket> _startTcs;
        private readonly IObservable<Socket> _observable;
        private int _disposed;
        private readonly SocketFactory _socketAwaitableFactory;
        private Func<Socket> _createListenerFunc;
        private SocketAwaitableEventArgs _awaitable;

        public SocketListener(Func<Socket> createFunc, SocketFactory awaitableFactory)
        {
            _createListenerFunc = createFunc;
            _startTcs = new TaskCompletionSource<Socket>();
            _socketAwaitableFactory = awaitableFactory;
            Func<IObserver<Socket>, Task<IDisposable>> loop = AcceptLoop;
            _observable = Observable.Create<Socket>(loop);
        }

        private bool IsDisposed
        {
            get { return _disposed != 0; }
        }

        public Task Start()
        {
            ThrowIfDisposed();
            var create = _createListenerFunc;
            if (Interlocked.CompareExchange(ref _createListenerFunc, null, create) != null)
            {
                try
                {                    
                    _listenerSocket = create();                    
                    Events.Trace.CreateSocketListener(_listenerSocket.LocalEndPoint.ToString());
                    _awaitable = _socketAwaitableFactory.GetSocketAwaitable();
                    _startTcs.SetResult(_listenerSocket);
                }
                catch (Exception ex)
                {
                    Events.Trace.Failure("Error creating socket listener." + ex.ToString());
                    _startTcs.SetException(ex);
                    throw;
                }
            }

            return _startTcs.Task;
        }

        private async Task<IDisposable> AcceptLoop(IObserver<Socket> observer)
        {
            Exception completionException = null;

            try
            {
                await Start();

                while (!IsDisposed)
                {
                    var socket = await _listenerSocket.AcceptSocketAsync(_awaitable);

                    observer.OnNext(socket);
                }
            }
            catch (Exception ex)
            {
                if (_awaitable.SocketError == SocketError.OperationAborted)
                {

                }

                completionException = ex;
            }
            finally
            {
                if (completionException != null)
                {
                    observer.OnError(completionException);
                }
                else
                {
                    observer.OnCompleted();
                }
            }

            return this;
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                try
                {
                    _listenerSocket.Close();
                }
                catch (Exception)
                {
                }
            }
        }

        public IDisposable Subscribe(IObserver<Socket> observer)
        {
            ThrowIfDisposed();
            _observable.Subscribe(observer);
            return this;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed != 0)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }
    }
}