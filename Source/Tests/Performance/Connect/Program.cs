using System.CodeDom;
using System.Reactive.Disposables;
using System.ServiceModel;
using System.ServiceModel.Channels;
using CmdLine;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Connect.Http;
using Connect.Sockets;
using Connect.WCF.Channels;

namespace Connect
{
    class Program
    {
        static void Main(string[] args)
        {
            SetupThreadPool();

            try
            {
                Console.WriteLine("PID:" + Process.GetCurrentProcess().Id);
                var arguments = CommandLine.Parse<ConnectArgs>();
                IDisposable resources = Disposable.Empty;

                if (System.String.Compare(arguments.Mode, "server", System.StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var r1 = StartServer(arguments);
                    resources = Disposable.Create(r1.Dispose);

                }
                else if (String.Compare(arguments.Mode, "client", true) == 0)
                {
                    var r1 = StartClients(arguments);
                    resources = Disposable.Create(r1.Dispose);
                }
                else
                {
                    var r1 = StartServer(arguments);
                    var r2 = StartClients(arguments);
                    resources = Disposable.Create(() =>
                    {
                        r1.Dispose();
                        r2.Dispose();
                    });
                }

                CommandLine.Pause();
                resources.Dispose();
            }
            catch (CommandLineException exception)
            {
                Console.WriteLine(exception.ArgumentHelp.Message);
                Console.WriteLine(exception.ArgumentHelp.GetHelpText(Console.BufferWidth));
            }
        }

        private static IDisposable StartClients(ConnectArgs arguments)
        {
            IClientManager clients = null;

            switch (arguments.Type)
            {
                case ConnectionType.socket:
                    clients = new SocketClientManager(arguments);
                    break;
                case ConnectionType.wcf:
                    clients = new DuplexChannelManager(arguments.ConnectionLimit,
                                                        arguments.MessageRate,
                                                        new NetTcpBinding() { Security = { Mode = SecurityMode.None } },
                                                        arguments.CreateNetTcpAddress());
                    break;

            }

            if (clients == null)
            {
                throw new NotImplementedException();
            }

            return clients.Start();
        }

        private static IDisposable StartServer(ConnectArgs arguments)
        {
            IServer server = null;

            switch (arguments.Type)
            {
                case ConnectionType.socket:
                    server = new SocketServer(arguments.Port);
                    break;
                case ConnectionType.wcf:
                    server = new TcpChannelServer(arguments.CreateNetTcpAddress());
                    break;
                case ConnectionType.http:
                    server = new HttpListenerServer("http://+:" + arguments.Port + "/");
                    break;
            }

            if (server == null)
            {
                throw new NotImplementedException();
            }
            //arguments.Server = Dns.GetHostName();

            return server.StartServer();
        }


        private static void SetupThreadPool()
        {
            if (!ExecutionContext.IsFlowSuppressed())
            {
                ExecutionContext.SuppressFlow();
            }

            ThreadPool.SetMinThreads(500, 1000);
        }

    }
}
