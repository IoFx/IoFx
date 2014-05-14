using System;
using System.IoFx.Connections;
using System.IoFx.Http;
using System.Net;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace Connect.Http
{
    class HttpListenerServer : IServer
    {
        ConnectionRateMonitor monitor = new ConnectionRateMonitor();
        private readonly IListener<System.Net.HttpListenerContext> _contexts;
        private readonly byte[] _buffer;
        public HttpListenerServer(string prefix)
        {
            _contexts = HttpObservable.GetContexts(prefix);
            const string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
            _buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            Console.WriteLine("Listening on " + prefix);
        }

        public IDisposable StartServer()
        {
            _contexts.Subscribe(ctx => Task.Run(async () =>
            {
                monitor.OnMessageStart();
                await SendResponse(ctx, _buffer);
                monitor.OnMessageEnd();
                monitor.OnMessage();
            }));

            monitor.Start();
            return Disposable.Create(_contexts.Dispose);
        }

        private static async Task SendResponse(HttpListenerContext context, byte[] buffer)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            await output.WriteAsync(buffer, 0, buffer.Length);
            await output.FlushAsync();
            output.Close();
        }
    }
}
