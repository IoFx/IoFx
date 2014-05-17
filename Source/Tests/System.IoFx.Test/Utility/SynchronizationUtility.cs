using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.IoFx.Test.Utility
{
    static class SynchronizationUtility
    {
        public static void WaitEx(this CountdownEvent handle)
        {
            if (Debugger.IsAttached)
            {
                handle.Wait();
            }
            else
            {
                handle.Wait(Defaults.MediumTestWaitTime);
            }
        }
    }
}
