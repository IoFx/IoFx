using System.Diagnostics.Contracts;
using System.IoFx.Connections;
using System.Reactive.Linq;

namespace System.IoFx.Framing
{
    public static class LengthPrefixedFramingExtensions
    {
        public static IObservable<Context<byte[]>> ToLengthPrefixed(this IConnection<ArraySegment<byte>> connection)
        {
            var outputTranslator = new LengthPrefixOutputTranslator(connection);
            var inputTranslator = new LengthPrefixedInputTranslator();
            var inputmessages = new TranslatorObservable<ArraySegment<byte>, byte[]>(connection, inputTranslator);
            return inputmessages.Select(message =>
                    {
                        return new Context<byte[]>
                        {
                            Message = message,
                            Channel = outputTranslator
                        };
                    });
        }

        struct LengthPrefixOutputTranslator : ITranslator<byte[], ArraySegment<byte>>, IConsumer<byte[]>
        {
            private readonly IConsumer<ArraySegment<byte>> _consumer;

            public LengthPrefixOutputTranslator(IConsumer<ArraySegment<byte>> consumer)
            {
                _consumer = consumer;
            }

            public void OnNext(byte[] value, IConsumer<ArraySegment<byte>> observer)
            {
                _consumer.Publish(new ArraySegment<byte>(value));
            }

            public void Publish(byte[] value)
            {
                this.OnNext(value, null);
            }
        }

        private class LengthPrefixedInputTranslator : ITranslator<ArraySegment<byte>, byte[]>
        {
            const int LengthPrefixSize = 4;
            private readonly PayloadReader<byte[]> _payloadReader;
            private readonly PayloadReader<int> _lengthPrefixReader;
            private State _state = State.ReadingPreamble;

            private enum State
            {
                ReadingPreamble,
                ReadingPayload,
            }

            public LengthPrefixedInputTranslator()
            {
                _lengthPrefixReader = new PayloadReader<int>(buffer => BitConverter.ToInt32(buffer, 0));
                _lengthPrefixReader.SetDataBuffer(new byte[LengthPrefixSize]); 
                _payloadReader = new PayloadReader<byte[]>(_ => _);

            }

            public void OnNext(ArraySegment<byte> item, IConsumer<byte[]> observer)
            {
                var current = item;
                while (current.Count > 0)
                {
                    if (_state == State.ReadingPreamble)
                    {
                        int size = 0;
                        if (_lengthPrefixReader.TryRead(ref current, out size))
                        {
                            // TODO: Change to array segment and use a pool of fixed size segments.
                            // start next payload extraction.
                            _payloadReader.SetDataBuffer(new byte[size]);
                            _state = State.ReadingPayload;
                        }
                    }

                    if (_state == State.ReadingPayload)
                    {
                        byte[] payload;
                        if (_payloadReader.TryRead(ref current, out payload))
                        {
                            observer.Publish(payload);

                            // Start next message;
                            _state = State.ReadingPreamble;
                        }
                    }
                }
            }

            private class PayloadReader<T>
            {
                readonly Func<byte[], T> _materialize;
                private int _dstNextFree;
                private byte[] _dst;
                private int _size;

                public PayloadReader(Func<byte[], T> materialize)
                {
                    _materialize = materialize;
                }

                public void SetDataBuffer(byte[] destination)
                {
                    Contract.Assert(destination != null);
                    _size = destination.Length;
                    _dst = destination;
                }

                public bool TryRead(ref ArraySegment<byte> source, out T result)
                {
                    var srcSegment = new ReaderSegment(source);
                    var pending = _size - _dstNextFree;
                    bool success;
                    if (srcSegment.CanRead(pending))
                    {
                        srcSegment.ReadInto(_dst, _dstNextFree, pending);
                        result = _materialize(_dst);
                        _dstNextFree = 0;  // Reset the destination offset
                        success = true;
                    }
                    else
                    {
                        Contract.Assert(pending > srcSegment.Count);
                        _dstNextFree += srcSegment.ReadInto(_dst, _dstNextFree);
                        Contract.Assert(srcSegment.Count == 0);
                        result = default(T);
                        success = false;
                    }

                    srcSegment.SetSegment(out source);
                    return success;
                }

                private struct ReaderSegment
                {
                    private readonly byte[] _array;
                    private int _offset;
                    private readonly int _last;

                    public ReaderSegment(ArraySegment<byte> data)
                    {
                        _array = data.Array;
                        _offset = data.Offset;
                        _last = data.Offset + data.Count - 1;
                    }

                    public bool CanRead(int size)
                    {
                        return Count >= size;
                    }

                    internal int Count
                    {
                        get { return _last - _offset + 1; }
                    }

                    int Move(int size)
                    {
                        Contract.Assert(Count >= size);
                        _offset += size;
                        return _offset;
                    }

                    public void SetSegment(out ArraySegment<byte> segment)
                    {
                        segment = new ArraySegment<byte>(_array, _offset, Count);
                    }

                    /// <summary>
                    /// Moves the data into the destination and returns the bytes read;
                    /// </summary>
                    /// <param name="dst"></param>
                    /// <param name="offset"></param>
                    /// <param name="count"></param>
                    /// <returns></returns>
                    public int ReadInto(byte[] dst, int offset, int count)
                    {
                        Buffer.BlockCopy(_array, _offset, dst, offset, count);
                        Move(count);
                        return count;
                    }

                    /// <summary>
                    /// Read all pending data into the destination
                    /// </summary>
                    /// <param name="dst"></param>
                    /// <param name="offset"></param>
                    /// <returns></returns>
                    public int ReadInto(byte[] dst, int offset)
                    {
                        return ReadInto(dst, offset, this.Count);
                    }
                }
            }            
        }
    }
}
