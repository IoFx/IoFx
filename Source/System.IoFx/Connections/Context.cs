using System.Diagnostics.Contracts;
using System.IoFx.Sockets;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace System.IoFx.Connections
{
    public struct Context<T>
    {
        public T Data { get; set; }
        public IConnection<T> Channel { get; set; }

        public T Publish(T output)
        {
            Channel.OnNext(output);
            return output;
        }
    }


    public interface IProducer<T>
    {
        bool Get(ref T item);
    }

    public struct SingleItemProducer<T> :IProducer<T>
    {
        private T _data;
        private bool _taken;

        public SingleItemProducer(T data)
        {
            _data = data;
            _taken = false;
        }

        public bool Get(ref T item)
        {
            if (!_taken)
            {
                _taken = true;
                item = _data;
                _data = default(T);
                return true;
            }

            return false;
        }
    }
}
