using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connect
{
    interface IClientManager
    {
        IDisposable Start();
    }
}
