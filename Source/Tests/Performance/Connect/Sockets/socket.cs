using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connect.Sockets
{
    class socket : TestScenario
    {
        private SocketClientManager clients;
        private SocketServer server;
        public override void Run()
        {
            if (Arguments.IsServer())
            {
                server = new SocketServer(Arguments.Port);
                server.StartServer();
            }
            else
            {
                clients = new SocketClientManager(Arguments);
                clients.Start();
            }
        }
    }
}
