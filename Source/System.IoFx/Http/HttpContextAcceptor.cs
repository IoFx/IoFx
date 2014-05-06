using System.IoFx.Connections;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.IoFx.Http
{
    class HttpContextAcceptor : IListener<HttpListenerContext>
    {
        private readonly HttpListener _listener;
        private int _disposed;
        private readonly IObservable<HttpListenerContext> _observable;

        public HttpContextAcceptor(HttpListener listener)
        {
            _listener = listener;

            Func<IObserver<HttpListenerContext>, Task<IDisposable>> loopFunc = ContextReceiveLoop;
            _observable = Observable.Create(loopFunc);
        }

        private async Task<IDisposable> ContextReceiveLoop(IObserver<HttpListenerContext> observer)
        {
            Exception completionException = null;

            try
            {
                await Start();

                while (true)
                {
                    if (!_listener.IsListening)
                    {
                        break;
                    }

                    var context = await _listener.GetContextAsync();

                    observer.OnNext(context);
                }
            }
            catch (HttpListenerException ex)
            {
                //TODO: handle http exceptions gracefully
                completionException = ex;
            }
            catch (Exception ex)
            {
                completionException = ex;
            }
            finally
            {
                if (completionException == null)
                {
                    observer.OnCompleted();
                }
                else
                {
                    observer.OnError(completionException);
                }
            }

            return Disposable.Create(this.Dispose);
        }

        public Task Start()
        {
            _listener.Start();
            return Task.FromResult(true);
        }

        public IDisposable Subscribe(IObserver<HttpListenerContext> observer)
        {
            return _observable.Subscribe(observer);
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                _listener.Stop();
            }
        }
    }
}