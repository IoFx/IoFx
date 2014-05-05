using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace System.IoFx.ServiceModel
{
    public static class BindingExtensions
    {
        public static IChannelListener<IDuplexSessionChannel> Start(this Binding binding, string uri)
        {
            return binding.Start<IDuplexSessionChannel>(uri);
        }

        public static IChannelListener<TChannel> Start<TChannel>(this Binding binding, string uri) where TChannel : class, IChannel
        {
            var listener = binding.BuildChannelListener<TChannel>(new Uri(uri));
            listener.Open();
            return listener;
        }
    }
}
