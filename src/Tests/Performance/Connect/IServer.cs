using System;
namespace Connect
{
    public interface IServer
    {
        IDisposable StartServer();
    }
}