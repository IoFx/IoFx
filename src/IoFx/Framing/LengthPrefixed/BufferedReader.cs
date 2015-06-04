using System;

namespace IoFx.Framing.LengthPrefixed
{
    struct BufferedReader
    {
        private int _readBytes;
        private ArraySegment<byte> _dst;
        private int _bufferSize;

        public ArraySegment<byte> Data
        {
            get { return _dst; }
        }

        public void StartRead(ArraySegment<byte> destination)
        {
            _bufferSize = destination.Count;
            _dst = destination;
            _readBytes = 0;
        }

        /// <summary>
        /// Reads the source to fill the reader and updates the 
        /// source with the remaining bytes.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool TryRead(ref ArraySegment<byte> source)
        {            
            var pending = _bufferSize - _readBytes;
            var count = Math.Min(pending, source.Count);

            if (count > 0)
            {
                Buffer.BlockCopy(
                    source.Array,
                    source.Offset,
                    _dst.Array,
                    _dst.Offset + _readBytes,
                    count);

                _readBytes += count;

                source = new ArraySegment<byte>(
                    source.Array,
                    source.Offset + count,
                    source.Count - count);
            }
            return _readBytes == _bufferSize;
        }
    }
}