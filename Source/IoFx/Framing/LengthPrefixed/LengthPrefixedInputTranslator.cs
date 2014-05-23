using System;

namespace IoFx.Framing.LengthPrefixed
{
    class LengthPrefixedInputTranslator : ITranslator<ArraySegment<byte>, byte[]>
    {
        private enum State
        {
            Preamble,
            Payload,
        }

        private struct Entry
        {
            public ArraySegment<byte> SizeBuffer;
            public ArraySegment<byte> PayloadBuffer;
        }

        private BufferedReader _payload;
        private BufferedReader _preamble;
        private State _state = State.Preamble;
        private Entry _entry;
        private ArraySegment<byte> _current;

        public LengthPrefixedInputTranslator()
        {
            _preamble = new BufferedReader();
            _entry.SizeBuffer = new ArraySegment<byte>(new byte[4]);
            _preamble.StartRead(_entry.SizeBuffer);
            _payload = new BufferedReader();
        }

        public void OnNext(ArraySegment<byte> item, IConsumer<byte[]> observer)
        {
            _current = item;
            while (_current.Count > 0)
            {
                if (_state == State.Preamble)
                {
                    if (_preamble.TryRead(ref _current))
                    {
                        InitializePayload();
                        _state = State.Payload;
                    }
                }

                if (_state == State.Payload)
                {
                    if (_payload.TryRead(ref _current))
                    {
                        _entry.PayloadBuffer = _payload.Data;
                        observer.Publish(_entry.PayloadBuffer.Array);
                        Cleanup();
                        _state = State.Preamble;
                    }
                }
            }
        }

        private void InitializePayload()
        {
            int size = BitConverter.ToInt32(
                _entry.SizeBuffer.Array,
                _entry.SizeBuffer.Offset);

            // TODO: Use arrays from pool.
            _entry.PayloadBuffer = new ArraySegment<byte>(new byte[size]);
            _payload.StartRead(_entry.PayloadBuffer);
        }

        private void Cleanup()
        {
            Array.Clear(
                _entry.SizeBuffer.Array,
                _entry.SizeBuffer.Offset,
                _entry.SizeBuffer.Count);
            _entry.PayloadBuffer = default(ArraySegment<byte>);

            // TODO: lifetime management of the payload. 
            // We might need to return the buffer to the pool.      
            _preamble.StartRead(_entry.SizeBuffer);
            _payload.StartRead(_entry.PayloadBuffer);
        }
    }
}
