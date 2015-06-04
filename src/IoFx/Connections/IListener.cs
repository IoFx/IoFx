using System.Threading.Tasks;

namespace IoFx.Connections
{
    public interface IListener
    {
        Task Start();
    }

    public interface IListener<out TObservable> : IListener, IDisposableObservable<TObservable>
    {
        
    }
}
