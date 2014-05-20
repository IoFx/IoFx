using System.ServiceModel.Channels;

namespace IoFx.ServiceModel
{
    class OutputChannelMessageConsumer : IConsumer<Message>
    {
        private readonly IOutputChannel _channel;

        public OutputChannelMessageConsumer(IOutputChannel channel)
        {            
            this._channel = channel;
        }

        public void Publish(Message item)
        {
            // TODO: Enable publishing the work to the queue.
            _channel.Send(item);
        }
    }
}
