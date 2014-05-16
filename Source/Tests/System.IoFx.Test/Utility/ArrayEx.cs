using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IoFx.Test.Utility
{
    static class ArrayEx
    {
        public static T[] Prepend<T>(this T[] x, T[] data)
        {
            Contract.Assert(x != null && data != null);
            int combined = x.Length + data.Length;
            var dst = new T[combined];
            Array.ConstrainedCopy(data, 0, dst, 0, data.Length);
            Array.ConstrainedCopy(x, 0, dst, data.Length, x.Length);
            return dst;
        }
    }
}
