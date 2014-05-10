using System;
using System.Collections.Generic;
using System.IO;
using System.IoFx.Connections;
using System.IoFx.Framing;
using System.IoFx.Sockets;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Samples.sockets
{
    class SimpleSocketServer
    {

        private IDisposable StartServer()
        {
            var listener = SocketObservable.CreateTcpStreamListener(5050);
           
            return listener
                .Subscribe(connection =>
                {
                    var messages = connection.ToFixedLenghtMessages();
                    messages.Subscribe(m =>
                    {
                        Console.WriteLine(m.Data.Length);
                    });
                });
        }
    }
}
