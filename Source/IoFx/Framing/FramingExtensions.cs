using System;
using IoFx.Connections;
using IoFx.Framing.LengthPrefixed;

namespace IoFx.Framing
{
    public static class FramingExtensions
    {
        public static IConnection<Context<byte[]>, byte[]> ToLengthPrefixed(this IConnection<ArraySegment<byte>> connection)
        {
            var outputTranslator = new LengthPrefixOutputTranslator();
            var inputTranslator = new LengthPrefixedInputTranslator();
            var txConn = new ConnectionTranslator<ArraySegment<byte>, byte[]>(
                connection,
                inputTranslator,
                outputTranslator);

            return txConn.AsContexts();
        }
    }
}
