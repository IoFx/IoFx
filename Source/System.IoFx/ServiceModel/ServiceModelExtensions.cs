namespace System.IoFx.ServiceModel
{
    using System.Reactive.Linq;
    using System.IoFx;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    public static class ServiceModelExtensions
    {
        public static IObservable<IoUnit<TPipelineType>> OnOperation<TPipelineType, TInput, TOuptput>(
            this IObservable<IoUnit<TPipelineType>> iochannel,
            Func<TPipelineType, bool> filter,
            Func<TInput, TOuptput> operation,
            Func<TPipelineType, TInput> decode,
            Func<TOuptput, TPipelineType, TPipelineType> encode)
        {
            var responses = iochannel
                .Where(unit => filter(unit.Unit))
                .Select(unit =>
                    {
                        var input = decode(unit.Unit);
                        var output = operation(input);
                        var outputMsg = encode(output, unit.Unit);
                        return new IoUnit<TPipelineType>()
                        {
                            Unit = outputMsg,
                            Parent = unit.Parent
                        };
                    });

            return responses;
        }

         
        public static IDisposable Consume(this IObservable<IoUnit<Message>> responses)
        {
            // Subscription involves sending the response back. 
            return responses.Subscribe(r => r.Publish(r.Unit));
        }

        public static IDisposable Consume(this IObservable<Task<IoUnit<Message>>> responses)
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
