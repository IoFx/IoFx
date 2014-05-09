using System.CodeDom;
using System.IoFx.Connections;
using System.IoFx.Runtime;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.IoFx.Framing
{
    public static class FramingExtensions
    {
        public static IObservable<Frame<T>> Receive<T>(this IObservable<ArraySegment<byte>> data)
        {
            throw new NotImplementedException();
        }


        private class SizeDelimitedReader : IConnector<Context<byte[]>, ArraySegment<byte>>
        {

            private IObservable<Context<byte[]>> _messages;
            private IDisposableObservable<ArraySegment<byte>> _inputs;
            private const int SIZE_FIELD_LENGTH = 4; // 32-bit integer
            private ArraySegment<byte> _current;


            public SizeDelimitedReader(IDisposableObservable<ArraySegment<byte>> inputs)
            {
                _inputs = inputs;
                Func<IObserver<Context<byte[]>>, Task<IDisposable>> receiveLoop = ReceiveMessage;
                _messages = Observable.Create(receiveLoop);
            }


            private async Task<IDisposable> ReceiveMessage(IObserver<Context<byte[]>> observer)
            {

                /*
                 *  _preamble.Subscribe( c => 
                 *      c.GetBody().Subscribe(
                 *  var data = await Body()
                 * 
                 */

                _inputs.SelectMany();

                return _inputs;
            }


            private static byte[] GetBuffer(long size)
            {
                return new byte[size];
            }


            private class PreambleSelector
            {
                readonly byte[] _data = new byte[4];
                private const int PreambleSize = 4;
                private int _read = 0;

                private bool TryRead(ref ArraySegment<byte> data, ref int preamble)
                {
                    var reader = new SegmentReader(data);
                    var pending = 4 - _read;
                    if (reader.CanRead(pending))
                    {
                        Buffer.BlockCopy(reader.Buffer, reader.First, _data, _read, pending);
                        preamble = BitConverter.ToInt32(_data, 0);
                        data = new ArraySegment<byte>(data.Array, reader.Read(pending), reader.Count);
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
                public byte[] Buffer;
                public int First;
                public int Last;

                public SegmentReader(ArraySegment<byte> data)
                {
                    Buffer = data.Array;
                    First = data.Offset;
                    Last = data.Offset + data.Count - 1;
                }

                public bool CanRead(int size)
                {
                    return Count >= size;
                }

                public int Count
                {
                    get { return Last - First + 1; }
                }

                public int Offset
                {
                    get { return Last + 1; }
                }

                public int Read(int size)
                {
                    First += size;
                    return First;
                }
            }


            private class SegmentIntReader : IAwaiter<int>
            {
                private SegmentReader segment;
                private const int FieldSize = 4;
                private bool _completed;
                private int _result;
                private byte[] _data;

                public void Set(ArraySegment<byte> data)
                {
                    segment = new SegmentReader(data);
                    _completed = segment.CanRead(FieldSize);
                    if (_completed)
                    {
                        _result = BitConverter.ToInt32(segment.Buffer, segment.First);
                    }
                    else
                    {

                    }
                }

                public IAwaiter<int> GetAwaiter()
                {
                    return this;
                }

                public int GetResult()
                {
                    if (!_completed)
                    {
                        throw new InvalidOperationException("Cannot block on a pure async reader.");
                    }

                    return _result;
                }

                public bool IsCompleted
                {
                    get
                    {
                        return _completed;
                    }
                }

                public void OnCompleted(Action continuation)
                {
                    continuation();
                }
            }

            IDisposable IObservable<Context<byte[]>>.Subscribe(IObserver<Context<byte[]>> observer)
            {
                return _messages.Subscribe(observer);
            }

            void IObserver<ArraySegment<byte>>.OnCompleted()
            {
                throw new NotImplementedException();
            }

            void IObserver<ArraySegment<byte>>.OnError(Exception error)
            {
                throw new NotImplementedException();
            }

            void IObserver<ArraySegment<byte>>.OnNext(ArraySegment<byte> value)
            {
                throw new NotImplementedException();
            }
        }
    }
}
