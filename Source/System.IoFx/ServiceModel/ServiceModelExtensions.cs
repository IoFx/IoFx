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
                .Where(unit => filter(unit.Unit))
                .Select(unit =>
                    {
                        var input = decode(unit.Unit);
                        var output = operation(input);
                        var outputMsg = encode(output, unit.Unit);
                        return new Context<TConnectionType>()
                        {
                            Unit = outputMsg,
                            Parent = unit.Parent
                        };
                    });

            return responses;
        }

         
        public static IDisposable Consume(this IObservable<Context<Message>> responses)
        {
            // Subscription involves sending the response back. 
            return responses.Subscribe(r => r.Publish(r.Unit));
        }

        public static IDisposable Consume(this IObservable<Task<Context<Message>>> responses)
        {
            // Subscription involves sending the response back. 
            return responses.Subscribe(async (task) =>
            {

                //TODO: Exception handling
                var item = await task;
                item.Publish(item.Unit);
            });
        }
    }
}
