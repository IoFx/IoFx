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
            return new LengthPrefixedFramingReader(connection);
        }

        private class LengthPrefixedFramingReader : Encoder<ArraySegment<byte>, Context<byte[]>>
        {
            const int LengthPrefixSize = 4;
            private PayloadReader<byte[]> _payloadReader;
            private PayloadReader<int> _lengthPrefixReader;
            private State _state = State.ReadingPreamble;

            private enum State
            {
                ReadingPreamble,
                ReadingPayload,
            }

            public LengthPrefixedFramingReader(IObservable<ArraySegment<byte>> inputs)
                : base(inputs)
            {
                _payloadReader = new PayloadReader<byte[]>(_ => _);
                _lengthPrefixReader = new PayloadReader<int>(buffer => BitConverter.ToInt32(buffer, 0));
                _lengthPrefixReader.SetDataBuffer(new byte[LengthPrefixSize]);
            }

            public override void OnNext(ArraySegment<byte> item, IObserver<Context<byte[]>> observer)
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
                            observer.OnNext(new Context<byte[]>()
                            {
                                Data = payload,
                            });

                            // Start next message;
                            _state = State.ReadingPreamble;
                        }
                    }
                }
            }

            private class PayloadReader<T>
            {
                static Func<byte[], byte[]> _identity = input => input;

                private int _read = 0;
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
                    var pending = _size - _read;
                    bool success;
                    if (reader.CanRead(pending))
                    {
                        Buffer.BlockCopy(reader.Buffer, reader.First, _dst, _read, pending);
                        source = new ArraySegment<byte>(source.Array, reader.Move(pending), reader.Count);
                        result = _materialize(_dst);
                        _read = 0;
                        success = true;
                    }
                    else
                    {
                        Contract.Assert(_dst.Length >= _read + reader.Count);
                        Buffer.BlockCopy(reader.Buffer, reader.First, _dst, _read, reader.Count);
                        _read += reader.Count;
                        source = new ArraySegment<byte>(source.Array, reader.Move(reader.Count), 0);
                        result = default(T);
                        success = false;
                    }

                    return success;
                }
            }

            private struct ReaderSegment
            {
                public readonly byte[] Buffer;
                public int First;
                private readonly int _last;

                public ReaderSegment(ArraySegment<byte> data)
                {
                    Buffer = data.Array;
                    First = data.Offset;
                    _last = data.Offset + data.Count - 1;
                }

                public bool CanRead(int size)
                {
                    return Count >= size;
                }

                public int Count
                {
                    get { return _last - First + 1; }
                }

                public int Offset
                {
                    get { return _last + 1; }
                }

                public int Move(int size)
                {
                    First += size;
                    return First;
                }
            }
        }
    }
}
