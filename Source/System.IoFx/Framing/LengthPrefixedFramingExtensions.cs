using System.CodeDom;
using System.Diagnostics.Contracts;
using System.IO;
using System.IoFx.Connections;
using System.IoFx.Runtime;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            private IConsumer<ArraySegment<byte>> _consumer;

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
            private IConnection<ArraySegment<byte>> _connection;

            private enum State
            {
                ReadingPreamble,
                ReadingPayload,
            }

            public LengthPrefixedInputTranslator()
            {
                _payloadReader = new PayloadReader<byte[]>(_ => _);
                _lengthPrefixReader = new PayloadReader<int>(buffer => BitConverter.ToInt32(buffer, 0));
                _lengthPrefixReader.SetDataBuffer(new byte[LengthPrefixSize]);
            }

            public void OnNext(ArraySegment<byte> item, IConsumer<byte[]> observer)
            {
                var current = item;
                while (current.Count > 0)
                {
                    int size = 0;
                    if (_state == State.ReadingPreamble)
                    {
                        if (_lengthPrefixReader.TryRead(ref current, out size))
                        {
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
                static Func<byte[], byte[]> _identity = input => input;

                private int _dstOffset = 0;
                private byte[] _dst;
                private int _size;

                readonly Func<byte[], T> _materialize;
                private Func<byte[], int> func;

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
                    var reader = new ReaderSegment(source);
                    var pending = _size - _dstOffset;
                    bool success;
                    if (reader.CanRead(pending))
                    {
                        reader.ReadInto(_dst, _dstOffset, pending);
                        result = _materialize(_dst);
                        _dstOffset = 0;  // Reset the destination offset
                        success = true;
                    }
                    else
                    {
                        Contract.Assert(_dst.Length >= _dstOffset + reader.Count);
                        _dstOffset += reader.ReadInto(_dst, _dstOffset);
                        Contract.Assert(reader.Count == 0);
                        result = default(T);
                        success = false;
                    }

                    reader.SetSegment(ref source);
                    return success;
                }
            }

            private struct ReaderSegment
            {
                public readonly byte[] Array;
                public int Offset;
                private readonly int _last;

                public ReaderSegment(ArraySegment<byte> data)
                {
                    Array = data.Array;
                    Offset = data.Offset;
                    _last = data.Offset + data.Count - 1;
                }

                public bool CanRead(int size)
                {
                    return Count >= size;
                }

                public int Count
                {
                    get { return _last - Offset + 1; }
                }

                public int Move(int size)
                {
                    Contract.Assert(Count >= size);
                    Offset += size;
                    return Offset;
                }

                public void SetSegment(ref ArraySegment<byte> segment)
                {
                    segment = new ArraySegment<byte>(Array, Offset, Count);
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
                    Buffer.BlockCopy(Array, Offset, dst, offset, count);
                    Move(count);
                    return count;
                }

                public int ReadInto(byte[] dst, int offset)
                {
                    return ReadInto(dst, offset, this.Count);
                }
            }
        }
    }
}
