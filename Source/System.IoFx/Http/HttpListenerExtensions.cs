﻿using System;
using System.Collections.Generic;
using System.IoFx.Connections;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IoFx.Http
{
    public static class HttpListenerExtensions
    {
        public static IObservable<HttpListenerContext> GetContexts(this HttpListener listener)
        {
            return Observable.Create<HttpListenerContext>(async o =>
            {
                try
                {
                    while (true)
                    {
                        var context = await listener.GetContextAsync();
                        o.OnNext(context);
                    }

                }
                finally
                {

                }

                return (IDisposable)null;
            });
        }


        class HttpListenerEx : IListener<HttpListenerContext>
        {
            public Task Start()
            {
                throw new NotImplementedException();
            }

            public IDisposable Subscribe(IObserver<HttpListenerContext> observer)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

    }
}
