using System.Diagnostics;
using System.Threading;

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
