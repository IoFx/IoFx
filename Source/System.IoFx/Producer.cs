using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

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
