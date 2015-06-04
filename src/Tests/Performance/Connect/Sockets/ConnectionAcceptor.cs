using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Connect
{
    class AcceptArgsPool
    {
        // Allocate into the LOH
        const int BufferSize = 85000;
        readonly byte[] _zeroAcceptBuffer = new byte[BufferSize];
        ConcurrentQueue<ArraySegment<Byte>> bufferList;
        private GCHandle _handle;
        const int BufferCount = 10;
        const int SingleBufferCount = 4;

        public AcceptArgsPool(ConcurrentQueue<ArraySegment<byte>> bufferList)
        {
            this.bufferList = bufferList;
            //Pin the buffer;
            this._handle = GCHandle.Alloc(_zeroAcceptBuffer);

            for (int i = 0; i < BufferCount; i++)
            {
                var buffer = new ArraySegment<byte>(_zeroAcceptBuffer, i * SingleBufferCount, SingleBufferCount);
                bufferList.Enqueue(buffer);
            }
        }

        ArraySegment<byte> Take()
        {
            ArraySegment<byte> buffer;
            if (bufferList.TryDequeue(out buffer))
            {
                return buffer;
            }

            throw new InvalidOperationException("We should never run out of accept buffers in the pool.");
        }

        void Return(ref ArraySegment<Byte> item)
        {
            bufferList.Enqueue(item);
        }
    }
}
