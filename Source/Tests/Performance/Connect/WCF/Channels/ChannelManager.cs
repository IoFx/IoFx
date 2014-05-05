using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Connect.WCF.Channels
{
    class ChannelManager : IClientManager
    {
        private int _rate;
        private int _connections;
        private IChannelFactory<IDuplexSessionChannel> _factory;
        private EndpointAddress _address;

        public ChannelManager(int connections, int messageRate, Binding binding, string address)
        {
            _connections = connections;
            _rate = messageRate;
            _factory = binding.BuildChannelFactory<IDuplexSessionChannel>();
            _address = new EndpointAddress(address);
            _factory.Open();
            
        }

        public IDisposable Start()
        {
            var connectionOpening=  new Task[_connections];
            for (int i = 0; i < _connections; i++)
            {
                var channel = _factory.CreateChannel(_address);
                var openTask = Task.Factory.FromAsync(channel.BeginOpen, channel.EndOpen, null);
                connectionOpening[i] = openTask;
            }

            Task.WaitAll(connectionOpening);



            
            return null;
        }
    }
}
