using System.IoFx.Connections;

namespace System.IoFx.ServiceModel
{
    using System.Reactive.Linq;
    using System.IoFx;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    public static class ServiceModelExtensions
    {
        public static IObservable<Context<TConnectionType>> OnOperation<TConnectionType, TInput, TOuptput>(
            this IObservable<Context<TConnectionType>> iochannel,
            Func<TConnectionType, bool> filter,
            Func<TInput, TOuptput> operation,
            Func<TConnectionType, TInput> decode,
            Func<TOuptput, TConnectionType, TConnectionType> encode)
        {
            var responses = iochannel
                .Where(unit => filter(unit.Data))
                .Select(unit =>
                    {
                        var input = decode(unit.Data);
                        var output = operation(input);
                        var outputMsg = encode(output, unit.Data);
                        return new Context<TConnectionType>()
                        {
                            Data = outputMsg,
                            Channel = unit.Channel
                        };
                    });

            return responses;
        }

         
        public static IDisposable Consume(this IObservable<Context<Message>> responses)
        {
            // Subscription involves sending the response back. 
            return responses.Subscribe(r => r.Publish(r.Data));
        }

        public static IDisposable Consume(this IObservable<Task<Context<Message>>> responses)
        {
            // Subscription involves sending the response back. 
            return responses.Subscribe(async (task) =>
            {

                //TODO: Exception handling
                var item = await task;
                item.Publish(item.Data);
            });
        }
    }
}
