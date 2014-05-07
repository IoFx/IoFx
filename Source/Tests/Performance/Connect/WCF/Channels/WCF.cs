using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Connect.WCF.Channels
{
    class WCF :TestScenario
    {
        private TcpChannelServer _server;
        private DuplexChannelManager _clients;
        public override void Run()
        {
            if (Arguments.IsServer())
            {
                _server = new TcpChannelServer(Arguments.CreateNetTcpAddress());
                _server.StartServer();
            }
            else
            {
                _clients  = new DuplexChannelManager(Arguments.ConnectionLimit,
                                                        Arguments.MessageRate,
                                                        new NetTcpBinding() { Security = { Mode = SecurityMode.None } },
                                                        Arguments.CreateNetTcpAddress());
                _clients.Start();
            }
        }
    }
}
