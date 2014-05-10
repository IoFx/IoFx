using System.CodeDom;
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
    public static class SizeDelimitedReaderExtensions
    {
        public static IObservable<Context<byte[]>> ToFixedLenghtMessages(this IConnection<ArraySegment<byte>> connection)
        {
            return new SizeDelimitedReader(connection);
        }

        private class SizeDelimitedReader : Encoder<ArraySegment<byte>, Context<byte[]>>
        {
            private ArraySegment<byte> _current;
            readonly PreambleSelector _preambleReader = new PreambleSelector();
            private State _state = State.ReadingPreamble;
            private enum State
            {
                ReadingPreamble,
                ReadingMessage,
            }

            public SizeDelimitedReader(IObservable<ArraySegment<byte>> inputs)
                : base(inputs)
            {
            }

            public override void OnNext(ArraySegment<byte> item, IObserver<Context<byte[]>> observer)
            {
                _current = item;
                int size = 0;
                if (_state == State.ReadingPreamble)
                {
                    if (_preambleReader.TryRead(ref _current, ref size))
                    {
                        _state = State.ReadingMessage;
                    }
                    else
                    {
                        return;
                    }
                }

                Console.WriteLine("Reading message");
                _state = State.ReadingPreamble;
                observer.OnNext(new Context<byte[]>()
                {
                    Data =  new byte[size],
                });                
            }

            private class PreambleSelector
            {
                private const int SizeFieldLength = 4; // 32-bit integer
                readonly byte[] _data = new byte[SizeFieldLength];
                private const int PreambleSize = SizeFieldLength;
                private int _read = 0;

                public bool TryRead(ref ArraySegment<byte> data, ref int preamble)
                {
                    var reader = new SegmentReader(data);
                    var pending = 4 - _read;
                    if (reader.CanRead(pending))
                    {
                        Buffer.BlockCopy(reader.Buffer, reader.First, _data, _read, pending);
                        preamble = BitConverter.ToInt32(_data, 0);
                        data = new ArraySegment<byte>(data.Array, reader.Read(pending), reader.Count);
                        _read = 0;
                        return true;
                    }
                    else
                    {
                        _read = reader.Count;
                        Buffer.BlockCopy(reader.Buffer, reader.First, _data, _read, reader.Count);
                        return false;
                    }
                }
            }

            private struct SegmentReader
            {
                public readonly byte[] Buffer;
                public int First;
                private readonly int _last;

                public SegmentReader(ArraySegment<byte> data)
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

                public int Read(int size)
                {
                    First += size;
                    return First;
                }
            }
        }
    }
}
