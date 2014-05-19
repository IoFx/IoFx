using System.Reactive;

namespace System.IoFx
{
    static class ProducerConsumerExtensions
    {
        public static IObserver<T> ToObserver<T>(this IConsumer<T> consumer)
        {
            return Observer.Create<T>(consumer.Publish);
        }
    }
}
