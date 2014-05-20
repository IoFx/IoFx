using System;
using System.Linq;
using System.Net;
using IoFx.Connections;

namespace IoFx.Http
{
    public class HttpObservable
    {
        public static IListener<HttpListenerContext> GetContexts(params string[] prefixes)
        {
            if (prefixes == null)
            {
                throw new ArgumentNullException("prefixes");
            }

            if (prefixes.Length == 0 || prefixes.Any(String.IsNullOrEmpty))
            {
                throw new ArgumentException("Invalid prefix specified.");
            }

            var listener = CreateListener(prefixes);
            return new HttpContextAcceptor(listener);
        }

        static HttpListener CreateListener(string[] prefixes)
        {
            var listener = new HttpListener();

            for (int i = 0; i < prefixes.Length; i++)
            {
                listener.Prefixes.Add(prefixes[i]);
            }

            return listener;
        }
    }
}
