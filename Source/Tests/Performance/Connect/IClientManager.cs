using System;

namespace Connect
{
    interface IClientManager
    {
        IDisposable Start();
    }
}
