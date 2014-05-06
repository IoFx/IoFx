using System;
using System.Collections.Generic;
using System.IoFx.Connections;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace System.IoFx.Http
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
            foreach (var prefix in prefixes)
            {
                listener.Prefixes.Add(prefix);
            }

            return listener;
        }
    }
}
