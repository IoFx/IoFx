using System;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IoFx.Sockets
{
    class SocketReceiver : IProducer<ArraySegment<byte>>, IDisposable
    {
        private readonly SocketFactory _factory;
        private readonly Socket _socket;
        private readonly IObservable<ArraySegment<byte>> _observable;
        private int _disposed;

        public SocketReceiver(Socket socket, SocketFactory factory)
        {
            _factory = factory;
            _socket = socket;
            Func<IObserver<ArraySegment<byte>>, Task<IDisposable>> loop = ReceiveLoop;
            _observable = Observable.Create(loop);
        }

        public IDisposable Subscribe(IObserver<ArraySegment<byte>> observer)
        {
            ThrowIfDisposed();
            return _observable.Subscribe(observer);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed != 0)
            {
                throw new ObjectDisposedException("SocketReceiver");
            }
        }

        private async Task<IDisposable> ReceiveLoop(IObserver<ArraySegment<byte>> observer)
        {
            var buffer = await _factory.Buffer.GetBufferAsync();
            var awaitable = _factory.GetSocketAwaitable();
            awaitable.SetBuffer(buffer, 0, buffer.Length);
            Exception completionException = null;

            try
            {
                while (_disposed == 0)
                {
                    await _socket.ReceiveSocketAsync(awaitable);
                    int count = awaitable.BytesTransferred;
                    if (count == 0)
                    {
                        break;
                    }

                    observer.OnNext(new ArraySegment<byte>(buffer, 0, count));
                }
            }
            catch (Exception ex)
            {
                completionException = ex;
            }
            finally
            {
                awaitable.Dispose();
                _factory.Buffer.Return(buffer);

                try
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
                catch (Exception)
                {
                    //TODO: Log
                }
            }

            return this;
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                _socket.Close();
            }
        }
    }
}