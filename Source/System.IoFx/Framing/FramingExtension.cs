using System.IoFx.Connections;

namespace System.IoFx.Framing
{
    public static class FramingExtensions
    {
        public static IObservable<Frame<T>> Receive<T>(this IObservable<ArraySegment<byte>> data)
        {
            throw new NotImplementedException();
        }
    }
}
