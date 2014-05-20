using System;
using System.Reactive.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using IoFx.Connections;

namespace IoFx.ServiceModel
{
    public static class ChannelExtensions
    {
        public static IProducer<TChannel> GetChannels<TChannel>(this IChannelListener<TChannel> listener) where TChannel : class, IChannel
        {
            //TODO: Need to throttle like max connections etc. 
            return new Acceptor<TChannel>(listener);
        }

        public static IProducer<Message> GetMessages<TChannel>(this TChannel channel) where TChannel : class, IInputChannel
        {
            Func<Task<Message>> receiveAsyncFunc = () => Task.Factory.FromAsync<Message>(
                                                        channel.BeginReceive,
                                                        channel.EndReceive, null);

            return new Receiver<TChannel>(channel, receiveAsyncFunc);
        }

        internal class Acceptor<TChannel> : IDisposable, IProducer<TChannel> where TChannel : class, IChannel
        {
            private readonly IChannelListener<TChannel> _listener;
            private readonly IObservable<TChannel> _observable;

            public Acceptor(IChannelListener<TChannel> listener)
            {
                _listener = listener;
                Func<IObserver<TChannel>, Task<IDisposable>> loop = AcceptLoop;
                _observable = Observable.Create(loop);
            }

            private async Task<IDisposable> AcceptLoop(IObserver<TChannel> channelObserver)
            {
                bool canContinue = true;
                Exception completedException = null;
                Func<Task<TChannel>> acceptFunc = () => Task.Factory.FromAsync<TChannel>(
                                                            _listener.BeginAcceptChannel,
                                                            _listener.EndAcceptChannel, null);

                while (canContinue)
                {
                    try
                    {
                        TChannel channel = await acceptFunc();
                        if (channel == null)
                        {
                            channelObserver.OnCompleted();
                            break;
                        }

                        channelObserver.OnNext(channel);

                    }
                    catch (Exception ex)
                    {
                        if (_listener.State == CommunicationState.Faulted)
                        {
                            channelObserver.OnError(ex);
                        }
                        else
                        {
                            channelObserver.OnCompleted();
                        }

                        canContinue = _listener.State == CommunicationState.Opened;
                    }
                }

                return this;
            }


            public void Dispose()
            {
                _listener.Abort();
            }

            public IDisposable Subscribe(IObserver<TChannel> observer)
            {
                return _observable.Subscribe(observer);
            }
        }

        internal class Receiver<TChannel> : IDisposable, IProducer<Message> where TChannel : class, IChannel
        {
            private readonly TChannel _channel;
            private readonly IObservable<Message> _observable;
            private readonly Func<Task<Message>> _receiveAsyncFunc;

            public Receiver(TChannel channel, Func<Task<Message>> receiveFunc)
            {
                if (channel == null)
                {
                    throw new ArgumentNullException("channel");
                }

                _channel = channel;
                _receiveAsyncFunc = receiveFunc;
                Func<IObserver<Message>, Task<IDisposable>> loop = ReceiveLoop;
                _observable = Observable.Create(loop);
            }

            async Task<IDisposable> ReceiveLoop(IObserver<Message> channelObserver)
            {

                try
                {
                    Func<Task> openAsyncFunc = () => Task.Factory.FromAsync(
                                                _channel.BeginOpen,
                                                _channel.EndOpen, null);

                    await openAsyncFunc();

                    while (_channel.State == CommunicationState.Opened)
                    {
                        var message = await _receiveAsyncFunc();

                        if (message == null)
                        {
                            _channel.Close();
                            break;
                        }

                        channelObserver.OnNext(message);
                    }

                    channelObserver.OnCompleted();
                }
                catch (Exception ex)
                {
                    channelObserver.OnError(ex);
                }

                return this;
            }

            public void Dispose()
            {
                _channel.Abort();
            }

            public IDisposable Subscribe(IObserver<Message> observer)
            {
                return _observable.Subscribe(observer);
            }
        }

        public static IConsumer<Message> GetConsumer<TChannel>(
            this TChannel channel) where TChannel : IOutputChannel
        {

           return new OutputChannelMessageConsumer(channel);           
        }

        public static IObservable<IConnection<Message>> OnConnect<TChannel>(
            this IChannelListener<TChannel> listener)
            where TChannel : class, IOutputChannel, IInputChannel
        {

            return listener
                .GetChannels()
                .Select(channel =>
                {
                    var inputs = channel.GetMessages();
                    var outputs = channel.GetConsumer();
                    return channel.CreateIoChannel(inputs, outputs);
                });
        }

        public static void OnConnect<TChannel>(
            this IChannelListener<TChannel> listener,
            Action<IConnection<Message>> onNext)
            where TChannel : class, IOutputChannel, IInputChannel
        {
            listener.OnConnect().Subscribe(onNext);
        }

        public static IObservable<Context<Message>> OnMessage(this IObservable<IConnection<Message>> channels)
        {
            Func<IConnection<Message>, IObservable<Context<Message>>> translator = connection =>
            {
                IObservable<Context<Message>> messages = connection.Select(message => new Context<Message>()
                {
                    Message = message,
                    Channel = connection,
                });

                return messages;
            };

            return channels.SelectMany(translator);
        }

        public static IObservable<Context<Message>> OnMessage(
            this IChannelListener<IDuplexSessionChannel> listener)
        {
            return listener.OnConnect().OnMessage();
        }

        private static IoChannel<T, TChannel> CreateIoChannel<T, TChannel>(
            this TChannel channel,
            IProducer<T> inputs,
            IConsumer<T> outputs)
            where TChannel : class, IChannel
        {
            return new IoChannel<T, TChannel>(inputs, outputs, channel);
        }

        class IoChannel<T, TChannel> : Connection<T> where TChannel : class, IChannel
        {
            public IoChannel(IProducer<T> inputs, IConsumer<T> outputs, TChannel channel)
                : base(inputs, outputs)
            {
                this.Channel = channel;
            }

            public TChannel Channel { get; set; }
        }
    }
}