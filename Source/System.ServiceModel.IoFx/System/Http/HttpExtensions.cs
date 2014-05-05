namespace System.IoFx.Http
{
    using System.Net;
    using System.Reactive.Linq;
    using System.IoFx;
    using System.IO;

    public static class HttpExtensions
    {

        static IObservable<HttpListenerContext> OnContext(this HttpListener listener)
        {            
            var contextAcceptor =  Observable.FromAsync(listener.GetContextAsync);
            return contextAcceptor.Repeat();
        }

        static void Message(HttpListenerContext context)
        {            
        }
    }

    class HttpContextMessage : IoPipeline<Stream, Stream>
    {
        public HttpContextMessage(IObservable<Stream> producer, IObserver<Stream> consumer) : 
            base(producer, consumer)
        {
        }
    }
}
