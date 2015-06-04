using System.Collections.Generic;
using System.Linq;

namespace Connect.Sockets
{
    static class Utility
    {
        public static byte[] GetCharBuffer(uint size)
        {            
            var buffer = new byte[size];
            int i = 0;
            foreach (var c in GetChars(size))
            {
                buffer[i++] = (byte)c;
            }

            return buffer;
        }

        public static IEnumerable<char> GetChars(uint size)
        {
            var chars = Enumerable.Range(0, (int)size).Select(i =>
            {
                return (char)((i % 26) + 65);
            });

            return chars;
        }

    }
}
