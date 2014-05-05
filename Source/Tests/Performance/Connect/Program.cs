using System.CodeDom;
using System.ServiceModel;
using System.ServiceModel.Channels;
using CmdLine;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
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
                if (System.String.Compare(arguments.Mode, "server", System.StringComparison.OrdinalIgnoreCase) == 0)
                {
                    StartServer(arguments);
                    
                }
                else if (String.Compare(arguments.Mode, "client", true) == 0)
                {
                    StartClients(arguments);                 
                }
                else
                {
                    StartServer(arguments);     
                    
                    StartClients(arguments);            
                }

                CommandLine.Pause();
            }
            catch (CommandLineException exception)
            {
                Console.WriteLine(exception.ArgumentHelp.Message);
                Console.WriteLine(exception.ArgumentHelp.GetHelpText(Console.BufferWidth));
            }
        }

        private static void StartClients(ConnectArgs arguments)
        {
            IClientManager clients = null;

            if (arguments.Type == ConnectionType.socket)
            {
                clients = new SocketClientManager(arguments);
            }


            switch (arguments.Type)
            {
                case ConnectionType.socket:
                    clients = new SocketClientManager(arguments);
                    break;
                case ConnectionType.wcf:                    
                    clients = new ChannelManager(arguments.ConnectionLimit, 
                                                arguments.MessageRate, 
                                                new NetTcpBinding(){ Security = {Mode = SecurityMode.None }},
                                                arguments.CreateNetTcpAddress()
                                                );
                    break;

            }

            clients.Start();
        }

        private static void StartServer(ConnectArgs arguments)
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

            }
            
            //arguments.Server = Dns.GetHostName();

            server.StartServer();
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
