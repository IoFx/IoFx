using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace System.IoFx.Connections
{
    public interface IConnection<TResult> :IObservable<TResult>, IDisposable
    {
        IObserver<TResult> Sender { get;}
    }
}
