using System;
using System.Reactive;

namespace IoFx
{
    static class ProducerConsumerExtensions
    {
        public static IObserver<T> ToObserver<T>(this IConsumer<T> consumer)
        {
            return Observer.Create<T>(consumer.Publish);
        }
    }
}
