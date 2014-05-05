namespace System.ServiceModel.IoFx
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    class Client<TChannel> where TChannel : class, IChannel
    {
        
    }

    class TcpClient<TChannel> : Client<TChannel> where TChannel : class, IChannel
    {
        static NetTcpBinding binding;
        public static Func<string, TChannel> CreateChannel { get; protected set; }

        static TcpClient()
        {
            binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.None;

            CreateChannel = 
                uri => {
                    IChannelFactory<TChannel> factory = binding.BuildChannelFactory<TChannel>();
                    factory.Open();
                    TChannel channel = factory.CreateChannel(new EndpointAddress(uri));
                    channel.Open();
                    return channel;
                };
        }
    }    
}
