using System.Threading.Tasks;

namespace System.IoFx.Connections
{
    public interface IListener
    {
        Task Start();
    }

    public interface IListener<out TObservable> : IListener, IDisposableObservable<TObservable>
    {
        
    }
}
