using System;

namespace IoFx.Framing.LengthPrefixed
{
    struct LengthPrefixOutputTranslator : ITranslator<byte[], ArraySegment<byte>>
    {
        public void OnNext(byte[] value, IConsumer<ArraySegment<byte>> observer)
        {
            observer.Publish(new ArraySegment<byte>(BitConverter.GetBytes(value.Length)));
            observer.Publish(new ArraySegment<byte>(value));
        }
    }
}